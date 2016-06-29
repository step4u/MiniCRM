using System;
using System.Collections.Generic;
using System.Linq;

using Com.Huen.Libs;
using Com.Huen.DataModel;
using Com.Huen.Sql;

using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NAudio.Wave;

using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;
using System.Windows;

namespace Com.Huen.Sockets
{
    public delegate void RecordRequestedSuccessOnNatEventHandler(object obj, CommandMsg msg);

    public class RTPRecorderCouple : IDisposable
    {
        public event RecordRequestedSuccessOnNatEventHandler RecordRequestedSuccessOnNatEvent;

        private string userdatapath = string.Empty;
        private CONNECTED_MODE connectedmode = CONNECTED_MODE.NONE;
        private CRAgentOption _option = null;

        private System.Timers.Timer timer;

        private UdpClient client = null;
        private int localport = 21010;
        private Thread sockthread;
        private int remoteport = 21011;
        private IPEndPoint ipep;

        private WaveFormat pcmFormat = new WaveFormat(8000, 16, 1);
        private string seqnum = string.Empty;
        private List<RtpRecordInfo> RecordIngList;

        public RTPRecorderCouple() : this(21011, CONNECTED_MODE.PUBLIC)
        {
        }

        public RTPRecorderCouple(CONNECTED_MODE mode) : this(21011, mode)
        {
        }

        public RTPRecorderCouple(int rport, CONNECTED_MODE mode)
        {
            remoteport = rport;
            connectedmode = mode;

            Init();

            StartSocket();
        }

        private void Init()
        {
            RecordIngList = new List<RtpRecordInfo>();
            string path = util.GetRecordFolder();

            _option = new CRAgentOption();
            _option.SaveDirectory = path;
            _option.SaveFileType = "wav";
        }


        private void Sessiontimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            KeepSession();
        }

        public void RequestOnNat()
        {
            //CommandMsg msg = new CommandMsg() {
            //    cmd = USRSTRUCTS.ENABLE_NAT_CALL_RECORD_REQ,
            //    type = USRSTRUCTS.TYPE_COUPLEPHONE,
            //    direct = USRSTRUCTS.DIRECT_OUTGOING,
            //    from_ext = CoupleModeInfo.userid
            //};

            CommandMsg msg = new CommandMsg()
            {
                cmd = USRSTRUCTS.SEND_RECORD_DUMMY_DATA,
                from_ext = CoupleModeInfo.userid
            };

            byte[] bytes = util.GetBytes(msg);

            try
            {
                client.Send(bytes, bytes.Length);
            }
            catch (SocketException ex)
            {
                util.WriteLog(ex.ErrorCode, ex.Message.ToString());
            }
        }

        private void KeepSession()
        {
            //CommandMsg msg = new CommandMsg()
            //{
            //    cmd = USRSTRUCTS.SEND_RECORD_DUMMY_DATA,
            //    type = USRSTRUCTS.TYPE_COUPLEPHONE,
            //    from_ext = CoupleModeInfo.userid,
            //    userid = CoupleModeInfo.userid
            //};

            CommandMsg msg = new CommandMsg()
            {
                cmd = USRSTRUCTS.SEND_RECORD_DUMMY_DATA,
                from_ext = CoupleModeInfo.userid
            };

            byte[] bytes = util.GetBytes(msg);

            try
            {
                client.Send(bytes, bytes.Length);
            }
            catch (SocketException e)
            {
                util.WriteLog(e.ErrorCode, e.Message.ToString());
            }
        }

        private void StartSocket()
        {
            try
            {
                IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(CoupleModeInfo.pbxipaddress), remoteport);
                client = new UdpClient(localport);
                client.Connect(remoteEp);

                sockthread = new Thread(new ThreadStart(ReceiveMessage));
                sockthread.IsBackground = true;
                sockthread.Start();

                if (sockthread.IsAlive)
                {
                    if (connectedmode == CONNECTED_MODE.NAT)
                    {
                        // RequestOnNat();
                        timer = new System.Timers.Timer();
                        timer.Interval = 40000;
                        timer.Elapsed += Sessiontimer_Elapsed;
                        timer.Start();

                        this.KeepSession();
                    }
                }
            }
            catch (SocketException e)
            {
                util.WriteLog(e.ErrorCode, e.Message);
            }
            catch (Exception e)
            {
                util.WriteLog(e.Message);
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                // ipep = new IPEndPoint(IPAddress.Any, 0);
                ipep = new IPEndPoint(IPAddress.Parse(CoupleModeInfo.pbxipaddress), remoteport);

                while (true)
                {
                    byte[] buffer = null;
                    buffer = client.Receive(ref ipep);

                    if (buffer.Length > 0)
                    {
                        DoItFromSocketMessage(buffer);
                    }
                }
            }
            catch (SocketException e)
            {
                util.WriteLog(e.ErrorCode, e.Message);
            }
            //catch (Exception e)
            //{
            //    util.WriteLog(e.Message);
            //}
        }

        void DoItFromSocketMessage(byte[] buffer)
        {
            CommandMsg msg = util.GetObject<CommandMsg>(buffer);

            //if (msg.cmd == USRSTRUCTS.ENABLE_NAT_CALL_RECORD_RES)
            //{
            //    timer = new System.Timers.Timer();
            //    timer.Interval = 40000;
            //    timer.Elapsed += Sessiontimer_Elapsed;
            //    timer.Start();

            //    if (RecordRequestedSuccessOnNatEvent != null)
            //        RecordRequestedSuccessOnNatEvent(this, msg);

            //    return;
            //}

            RecordInfo_t recInfo = util.GetObject<RecordInfo_t>(buffer);

            int nDataSize = recInfo.size - 12;

            if (nDataSize != 80 && nDataSize != 160 && nDataSize != 240 && nDataSize != 10 && nDataSize != -12) return;

            // util.WriteLog(string.Format("seq:{0}, ext:{1}, peer:{2}, isExtension:{3}, size:{4}, bytesLength:{5}", recInfo.seq, recInfo.extension, recInfo.peer_number, recInfo.isExtension, recInfo.size - 12, recInfo.voice.Length));

            this.StackRtp2Instance(recInfo);
        }

        private void StackRtp2Instance(RecordInfo_t _recInfo)
        {
            var _ingInstance = RecordIngList.FirstOrDefault(x => x.ext == _recInfo.extension && x.peer == _recInfo.peer_number);
            if (_ingInstance == null)
            {
                byte[] rtpbuff = new byte[_recInfo.size];
                Array.Copy(_recInfo.voice, 0, rtpbuff, 0, _recInfo.size);
                WinSound.RTPPacket rtp = new WinSound.RTPPacket(rtpbuff);

                WaveFormat _wavformat;

                switch (rtp.PayloadType)
                {
                    case 0:
                        _wavformat = WaveFormat.CreateMuLawFormat(8000, 1);
                        break;
                    case 8:
                        _wavformat = WaveFormat.CreateALawFormat(8000, 1);
                        break;
                    case 4:
                        _wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.G723, 8000, 1, 8000 * 1, 1, 8);
                        break;
                    case 18:
                        _wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.G729, 8000, 1, 8000 * 1, 1, 8);
                        break;
                    default:
                        _wavformat = WaveFormat.CreateALawFormat(8000, 1);
                        break;
                }

                DateTime now = DateTime.Now;
                TimeSpan ts = now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

                string _header = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}{6:000}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
                string _datepath = string.Format("{0:0000}-{1:00}-{2:00}", now.Year, now.Month, now.Day);
                string _fileName = string.Format("{0}_{1}_{2}.wav", _header, _recInfo.extension, _recInfo.peer_number);
                string _wavFileName = string.Format(@"{0}\{1}\{2}", _option.SaveDirectory, _datepath, _fileName);

                string _path = string.Format(@"{0}\{1}", _option.SaveDirectory, _datepath);
                if (!Directory.Exists(_path))
                    Directory.CreateDirectory(_path);

                RtpRecordInfo RecInstance = new RtpRecordInfo(_wavformat, string.Format(@"{0}\{1}", _option.SaveDirectory, _datepath), _fileName) { ext = _recInfo.extension, peer = _recInfo.peer_number, codec = _wavformat, idx = ts.TotalMilliseconds, savepath = string.Format(@"{0}\{1}", _option.SaveDirectory, _datepath), filename = _fileName };

                RecInstance.EndOfRtpStreamEvent += RecInstance_EndOfRtpStreamEvent;

                RecInstance.Add(_recInfo);
                lock (RecordIngList)
                {
                    RecordIngList.Add(RecInstance);
                }
            }
            else
            {
                _ingInstance.Add(_recInfo);
            }
        }

        void RecInstance_EndOfRtpStreamEvent(object sender, EventArgs e)
        {
            RtpRecordInfo ingdata = (RtpRecordInfo)sender;

            if (ingdata != null)
            {
                double _idx = ingdata.idx;
                string _savepath = ingdata.savepath;
                string _savefn = ingdata.filename;
                string _ext = ingdata.ext;
                string _peer = ingdata.peer;

                lock (RecordIngList)
                {
                    RecordIngList.Remove(ingdata);
                }

                ingdata.Dispose();
                ingdata = null;

                if (_option.SaveFileType == "MP3")
                {
                    _savefn = this.LameWavToMp3(_savepath, _savefn);
                }

                // double _fnlen = FilePlayTime(string.Format(@"{0}\{1}", _savepath, _savefn));

                // this.FileName2DB(_idx, _savefn, _fnlen, _ext, _peer);
            }
            else
            {
                util.WriteLog("RecInstance_EndOfRtpStreamEvent ingdata : NULL");
            }
        }

        private string LameWavToMp3(string savepath, string wavFN)
        {
            string fullwavfn = string.Empty;
            string fullmp3fn = string.Empty;

            try
            {
                fullwavfn = string.Format("{0}\\{1}", savepath, wavFN);
                fullmp3fn = string.Format("{0}\\{1}", savepath, wavFN.Replace(".wav", ".mp3"));

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = @".\lame.exe";
                psi.Arguments = "-h -b 32 " + fullwavfn + " " + fullmp3fn;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(psi);
                p.WaitForExit();

                if (File.Exists(fullwavfn)) File.Delete(fullwavfn);
            }
            catch (Exception e)
            {
                util.WriteLog(e.Message);
                //throw e;
            }

            return fullmp3fn;
        }

        private void FileName2DB(double idx, string fn, double fnlen, string ext, string peernum)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                db.SetParameters("@EXTENTION", FbDbType.VarChar, ext);
                db.SetParameters("@PEERNUMBER", FbDbType.VarChar, peernum);
                db.SetParameters("@FN", FbDbType.VarChar, fn);
                db.SetParameters("@FNLEN", FbDbType.Double, fnlen);
                
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("INS_RECINFO3");
                    db.Commit();
                }
                catch (FbException e)
                {
                    util.WriteLog(string.Format("SQL INS ERROR (INS_RECINFO3)\r\nMessage : {0}", e.Message));
                    db.Rollback();
                }
            }
        }

        public void Close()
        {
            if (sockthread != null)
            {
                if (sockthread.IsAlive)
                {
                    sockthread.Abort();
                    sockthread = null;
                }
            }

            List<RtpRecordInfo> lastlist = RecordIngList.ToList();
            foreach (RtpRecordInfo item in lastlist)
            {
                this.RecInstance_EndOfRtpStreamEvent(item, new EventArgs());
            }

            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

            if (timer != null)
            {
                timer.Close();
                timer.Dispose();
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

            if (timer != null)
            {
                timer.Close();
                timer.Dispose();
            }
        }

        private double FilePlayTime(string _fullname)
        {
            double _len = 0.0d;

            if (File.Exists(_fullname))
            {
                if (_option.SaveFileType.ToLower() == "wav")
                {
                    WaveFileReader reader = new WaveFileReader(_fullname);
                    TimeSpan span = reader.TotalTime;
                    _len = span.TotalSeconds;
                }
                else
                {
                    AudioFileReader reader = new AudioFileReader(_fullname);
                    TimeSpan span = reader.TotalTime;
                    _len = span.TotalSeconds / 2;
                }
            }

            //util.WriteLog(string.Format("{0} / {1}", _fullname, span.TotalSeconds));

            return _len;
        }
    }
}
