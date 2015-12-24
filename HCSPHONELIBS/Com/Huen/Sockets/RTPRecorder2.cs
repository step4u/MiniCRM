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

namespace Com.Huen.Sockets
{
    public class RTPRecorder2 : IDisposable
    {
        private ModifyRegistry _reg = null;
        private CRAgentOption _option = null;

        private HUDPClient client = null;
        private Socket _sockRTPSrv = null;
        private Socket _sockCmdSrv = null;

        private WaveFormat pcmFormat = new WaveFormat(8000, 16, 1);

        private string seqnum = string.Empty;

        private List<RtpRecordInfo> RecordIngList;

        private EndPoint _localep;
        private EndPoint _remoteep;
        private EndPoint _localepCmd;
        private EndPoint _remoteepCmd;

        private bool _IsSockInterceptorStarted = false;
        private bool _IsSockCmdSrvStarted = false;

        private Thread _threadIntercept;

        private List<InterceptorClient> _cmdClientList = new List<InterceptorClient>();
        private List<InterceptorClient> _rtpRedirectClientList = new List<InterceptorClient>();
        private List<IPEndPoint> _fileClientList = new List<IPEndPoint>();

        private ObservableCollection<InnerTel> _innertelstatus = new ObservableCollection<InnerTel>();

        public RTPRecorder2() : this (MsgKinds.RecordInfo, 21010)
        {
        }

        public RTPRecorder2(MsgKinds __msgkinds, int __port)
        {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadRegistry));

            RecordIngList = new List<RtpRecordInfo>();

            client = new HUDPClient();
            client.UDPClientEventReceiveMessage += client_UDPClientEventReceiveMessage;

            client.SocketMsgKinds = __msgkinds;
            client.ServerPort = __port;
        }

        private void LoadRegistry(object state)
        {
            _reg = new ModifyRegistry(util.LoadProjectResource("REG_SUBKEY_CALLRECORDER", "COMMONRES", "").ToString());
            byte[] __bytes = (byte[])_reg.GetValue(RegKind.LocalMachine, "CR");
            _option = (CRAgentOption)util.ByteArrayToObject(__bytes);

            Thread.Sleep(2000);
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadRegistry));
        }

        #region RTP Redirect Server s
        public void StartRtpRedirectSrv()
        {
            _localep = new IPEndPoint(IPAddress.Any, 21020);
            _remoteep = new IPEndPoint(IPAddress.Any, 0);

            _sockRTPSrv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _sockRTPSrv.Bind(_localep);

            _IsSockInterceptorStarted = true;

            _threadIntercept = new Thread(new ThreadStart(RtpRedirectSendReceiver));
            _threadIntercept.IsBackground = true;
            _threadIntercept.Start();
        }

        public void StopRtpRedirectSrv()
        {
            _sockRTPSrv.Close();
            _sockRTPSrv = null;
        }

        private void RtpRedirectSendReceiver()
        {
            try
            {
                InterceptRequest __req;
                //InterceptRes __res;
                int __count = 0;

                while (_IsSockInterceptorStarted)
                {
                    __req = new InterceptRequest() { cmd = 0, extnum = "0000" };
                    //byte[] __bytes = util.GetBytes(__req);
                    byte[] __bytes = util.ObjectToByteArray(__req);

                    __count = 0;
                    __count = _sockRTPSrv.ReceiveFrom(__bytes, SocketFlags.None, ref _remoteep);

                    if (__count == 0) return;

                    //__req = util.GetObject<InterceptReq>(__bytes);
                    __req = util.ByteArrayToObject<InterceptRequest>(__bytes);

                    InterceptorClient __tmpObj = null;
                    int __idx = 0;
                    switch (__req.cmd)
                    {
                        case 2:
                            // RTP 클라이언트 등록 요청
                            //__res = new RTPInterceptRes() { cmd = __req.cmd, result = 1 };
                            //__bytes = util.GetBytes(__res);
                            //__count = _sockRTPSrv.SendTo(__bytes, _remoteep);

                            var __tmpList = _rtpRedirectClientList.Where(x => x.ClientIPEP.Address == ((IPEndPoint)_remoteep).Address);

                            if (__tmpList.Count() == 0)
                            {
                                lock (_rtpRedirectClientList)
                                {
                                    _rtpRedirectClientList.Add(new InterceptorClient() { ClientIPEP = (IPEndPoint)_remoteep, ClientRegdate = DateTime.Now, ReqtelNum = __req.extnum });
                                }
                            }
                            else
                            {
                                lock (_rtpRedirectClientList)
                                {
                                    __tmpObj = _rtpRedirectClientList.FirstOrDefault(x => x.ClientIPEP.Address == ((IPEndPoint)_remoteep).Address);
                                    __idx = _rtpRedirectClientList.IndexOf(__tmpObj);
                                    _rtpRedirectClientList[__idx].ClientRegdate = DateTime.Now;
                                    _rtpRedirectClientList[__idx].ReqtelNum = __req.extnum;
                                }
                            }
                            break;
                        case 3:
                            // RTP 클라이언트 등록 해제 요청
                            //__res = new RTPInterceptRes() { cmd = __req.cmd, result = 1 };
                            //__bytes = util.GetBytes(__res);
                            //__count = _sockRTPSrv.Send(__bytes, __bytes.Length, SocketFlags.None);
                            //__count = _sockRTPSrv.SendTo(__bytes, _remoteep);

                            __tmpObj = _rtpRedirectClientList.FirstOrDefault(x => x.ClientIPEP.Address == ((IPEndPoint)_remoteep).Address);
                            lock (_rtpRedirectClientList)
                            {
                                _rtpRedirectClientList.Remove(__tmpObj);
                            }
                            break;
                    }
                }
            }
            catch (SocketException se)
            {
                throw se;
            }
        }
        #endregion RTP Redirect Server e

        #region Command Server s
        public void StartCmdSrv()
        {
            _localepCmd = new IPEndPoint(IPAddress.Any, 21021);
            _remoteepCmd = new IPEndPoint(IPAddress.Any, 0);

            _sockCmdSrv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _sockCmdSrv.Bind(_localepCmd);

            _IsSockCmdSrvStarted = true;

            _threadIntercept = new Thread(new ThreadStart(CmdSendReceiver));
            _threadIntercept.IsBackground = true;
            _threadIntercept.Start();
        }

        public void StopCmdSrv()
        {
            _sockCmdSrv.Close();
            _sockCmdSrv = null;
        }

        private void CmdSendReceiver()
        {
            try
            {
                InterceptRequest __req = new InterceptRequest() { cmd = 0, extnum = string.Empty };
                InterceptResponse __res;
                int __count = 0;

                while (_IsSockCmdSrvStarted)
                {
                    //byte[] __bytes = util.GetBytes(__req);
                    byte[] __bytes = util.ObjectToByteArray(__req);
                    byte[] __hbytes = null;
                    byte[] __dbytes = null;
                    byte[] __sbytes = null;

                    __count = 0;
                    __count = _sockCmdSrv.ReceiveFrom(__bytes, SocketFlags.None, ref _remoteepCmd);

                    if (__count == 0) break;

                    //__req = util.GetObject<InterceptReq>(__bytes);
                    __req = util.ByteArrayToObject<InterceptRequest>(__bytes);

#if false
                    switch (__req.cmd)
                    {
                        case 1:
                            // 내선 전화 상태 요청
                            /*
                            __dbytes = util.ObjectToByteArray(_innertelstatus);
                            __res = new InterceptResponse() { cmd = __req.cmd, result = 1, length = __dbytes.Length };
                            __hbytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            */

                            __dbytes = util.ObjectToByteArray(_innertelstatus);
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1, length = __dbytes.Length };
                            __hbytes = util.ObjectToByteArray(__res);

                            this.CommandSendResponse(__hbytes, __dbytes, _sockCmdSrv, _remoteepCmd);



                            break;
                        case 2:
                            // RTP Redirect 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            //__bytes = util.GetBytes(__res);
                            __bytes = util.ObjectToByteArray(__res);
                            //__count = _sockRTPSrv.Send(__bytes, __bytes.Length, SocketFlags.None);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 3:
                            // RTP Redirect 해제 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            //__bytes = util.GetBytes(__res);
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 4:
                            // 파일 Stream 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            //__bytes = util.GetBytes(__res);
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteep);
                            break;
                        case 5:
                            // 파일 Down 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            //__bytes = util.GetBytes(__res);
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteep);
                            break;
                    }
#endif

                    switch (__req.cmd)
                    {
                        case 1:
                            // 내선 전화 상태 요청
                            __dbytes = util.ObjectToByteArray(_innertelstatus);
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1, length = __dbytes.Length };
                            __hbytes = util.ObjectToByteArray(__res);

                            this.CommandSendResponse(__hbytes, __dbytes, _sockCmdSrv, _remoteepCmd);
                            break;
                        case 2:
                            // RTP Redirect 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 3:
                            // RTP Redirect 해제 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 4:
                            // 파일 Stream 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 5:
                            // 파일 Down 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };
                            __bytes = util.ObjectToByteArray(__res);
                            __count = _sockCmdSrv.SendTo(__bytes, _remoteepCmd);
                            break;
                        case 6:
                            // Disk info 요청
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1 };

                            DiskInfo _diskinfo = new DiskInfo();
                            foreach (var driver in DriveInfo.GetDrives())
                            {
                                if (_option.SaveDirectory.Contains(driver.Name))
                                {
                                    _diskinfo.diskletter = driver.Name;
                                    _diskinfo.freespace = driver.TotalFreeSpace;
                                }
                            }
                            __dbytes = util.ObjectToByteArray(_diskinfo);
                            __res = new InterceptResponse() { cmd = __req.cmd, status = 1, length = __dbytes.Length };
                            __hbytes = util.ObjectToByteArray(__res);

                            this.CommandSendResponse(__hbytes, __dbytes, _sockCmdSrv, _remoteepCmd);
                            break;
                    }
                }
            }
            catch (SocketException se)
            {
                throw se;
            }
        }
        #endregion Command Server e

        public void StartServer()
        {
            client.StartServer();
        }

        public void StopServer()
        {
            client.Stop();
        }

        void client_UDPClientEventReceiveMessage(object sender, byte[] buffer)
        {
            switch (client.SocketMsgKinds)
            {
                case MsgKinds.CdrRequest:
                    //CdrRequest_t cdr_req = (CdrRequest_t)e;
                    //CdrList cdrlist = (CdrList)util.GetObject<CdrList>(cdr_req.data);

                    //HUDPClient cl = (HUDPClient)sender;
                    //cl.Send(2, MsgKinds.CdrResponse, cdr_req);
                    break;
                case MsgKinds.RecordInfo:
                    RecordInfo_t recInfo = util.GetObject<RecordInfo_t>(buffer);

                    int nDataSize = recInfo.size - 12;

                    if (nDataSize != 80 && nDataSize != 160 && nDataSize != 240 && nDataSize != -12) return;

                    util.WriteLog(string.Format("seq:{0}, ext:{1}, peer:{2}, isExtension:{3}, size:{4}, bytesLength:{5}", recInfo.seq, recInfo.extension, recInfo.peer_number, recInfo.isExtension, recInfo.size - 12, recInfo.voice.Length));

#if true
                    // 녹취 할 수 있는 내선 리스트 추가 및 상태 체크
                    var __tmpCollection = _innertelstatus.Where(x => x.Telnum == recInfo.extension);
                    if (__tmpCollection.Count() < 1)
                    {
                        lock (_innertelstatus)
                        {
                            _innertelstatus.Add(new InnerTel() { Telnum = recInfo.extension, TellerName = string.Empty, PeerNum = recInfo.peer_number, StartedDatetime = DateTime.Now, ElapsedSecond = 0, Status = 1 });
                        }
                    }
                    else
                    {
                        InnerTel __tmpinntel = _innertelstatus.FirstOrDefault(x => x.Telnum == recInfo.extension);
                        int __idx = _innertelstatus.IndexOf(__tmpinntel);
                        _innertelstatus[__idx].PeerNum = recInfo.peer_number;
                        TimeSpan _tmpts = DateTime.Now - _innertelstatus[__idx].StartedDatetime;
                        _innertelstatus[__idx].ElapsedSecond = (int)_tmpts.TotalSeconds;

                    }
                    // 내선 리스트 추가 및 상태 체크
#endif
#if false
                    // RTP Redirect
                    foreach (InterceptorClient __ic in _rtpRedirectClientList)
                    {
                        if (recInfo.extension == __ic.ReqtelNum)
                            _sockRTPSrv.SendTo(buffer, 0, buffer.Length, SocketFlags.None, __ic.ClientIPEP);
                    }
#endif
                    this.StackRtp2Instance(recInfo);
                    break;
            }
        }

        private void StackRtp2Instance(RecordInfo_t _recInfo)
        {
            var _ingInstance = RecordIngList.FirstOrDefault(x => x.ext == _recInfo.extension && x.peer == _recInfo.peer_number);
            if (_ingInstance == null)
            {
                WaveFormat _wavformat;

                switch (_recInfo.codec)
                {
                    case 0:
                        //_wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.MuLaw, 8000, 1, 8000, 1, 8);
                        _wavformat = WaveFormat.CreateMuLawFormat(8000, 1);
                        break;
                    case 8:
                        //_wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.ALaw, 8000, 1, 8000, 1, 8);
                        _wavformat = WaveFormat.CreateALawFormat(8000, 1);
                        break;
                    case 4:
                        _wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.G723, 8000, 1, 8000, 1, 8);
                        break;
                    case 18:
                        _wavformat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.G729, 8000, 1, 8000, 1, 8);
                        break;
                    default:
                        _wavformat = WaveFormat.CreateMuLawFormat(8000, 1);
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
                {
                    Directory.CreateDirectory(_path);
                }

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

                var __tmpCollection = _innertelstatus.FirstOrDefault(x => x.Telnum == ingdata.ext);
                if (__tmpCollection != null)
                {
                    lock (_innertelstatus)
                    {
                        _innertelstatus.Remove(__tmpCollection);
                    }
                }

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

                double _fnlen = FilePlayTime(string.Format("{0}\\{1}", _savepath, _savefn));

                this.FileName2DB(_idx, _savefn, _fnlen, _ext, _peer);
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
            /*
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@I_SEQ", idx);
            dt.Rows.Add("@EXTENTION", ext);
            dt.Rows.Add("@PEERNUMBER", peernum);
            dt.Rows.Add("@FN", fn);
            dt.Rows.Add("@FNLEN", fnlen);
            */

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn))
            {
                //db.SetParameters("@I_SEQ", FbDbType.Double, idx);
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
                catch (FirebirdSql.Data.FirebirdClient.FbException se)
                {
                    util.WriteLog(string.Format("SQL INS ERROR (INS_RECINFO2)\r\nMessage : {0}", se.Message));
                    db.Rollback();
                }
            }
        }

        public void Dispose()
        {
            this._sockRTPSrv.Close();
            this._sockCmdSrv.Close();

            this.client.Dispose();
            this._sockRTPSrv.Dispose();
            this._sockCmdSrv.Dispose();

            this._sockRTPSrv = null;
            this._sockCmdSrv = null;
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

        private void CommandSendResponse(byte[] headerBuffer, byte[] dataBuffer, Socket sock, EndPoint remoteEP)
        {
            byte[] sendBuffer = new byte[headerBuffer.Length + dataBuffer.Length];

            Array.Copy(headerBuffer, 0, sendBuffer, 0, headerBuffer.Length);
            Array.Copy(dataBuffer, 0, sendBuffer, headerBuffer.Length, dataBuffer.Length);

            int i = 0;
            int _sndlen = sendBuffer.Length;
            int _buffsize = 1024;
            while (_sndlen > 0)
            {
                byte[] _sndbuff = null;

                if (_sndlen < _buffsize)
                {
                    _sndbuff = new byte[_sndlen];
                }
                else
                {
                    _sndbuff = new byte[_buffsize];
                }

                Array.Copy(sendBuffer, i * _buffsize, _sndbuff, 0, _sndbuff.Length);
                int _count = sock.SendTo(_sndbuff, remoteEP);
                _sndlen -= _count;
                i++;
            }
        }
    }
}
