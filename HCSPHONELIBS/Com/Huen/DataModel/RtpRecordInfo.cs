using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Timers;
using System.IO;

using NAudio.Wave;
using Com.Huen.Sockets;

namespace Com.Huen.DataModel
{
    public delegate void EndOfRtpStreamEventHandler(object sender, EventArgs e);
    public class RtpRecordInfo : IDisposable
    {
        public event EndOfRtpStreamEventHandler EndOfRtpStreamEvent;

        private Timer timer;
        private Timer endtimer;
        private short endcount = 0;
        public double idx = 0.0d;
        public string ext = string.Empty;
        public string peer = string.Empty;
        public WaveFormat codec;
        public string savepath = string.Empty;
        public string filename = string.Empty;

        private List<ReceivedRtp> listIn;
        private List<ReceivedRtp> listOut;
        private WaveFileWriter writer = null;
        private WaveFormat pcmFormat = new WaveFormat(8000, 16, 1);

        //public RtpRecordInfo() : this (WaveFormat.CreateMuLawFormat(8000, 1), "", "")
        //{
        //}

        public RtpRecordInfo(WaveFormat _codec, string savepath, string filename)
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = now - (new DateTime(1970, 1, 1, 0, 0, 0, 0));
            idx = ts.TotalMilliseconds;
            codec = _codec;
            listIn = new List<ReceivedRtp>();
            listOut = new List<ReceivedRtp>();

            writer = new WaveFileWriter(string.Format(@"{0}\{1}", savepath, filename), pcmFormat);

            this.InitTimer();
        }

        private void InitTimer()
        {
            timer = new Timer();
            timer.Interval = 3000;
            timer.Enabled = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            endtimer = new Timer();
            endtimer.Interval = 5000;
            endtimer.Enabled = true;
            endtimer.Elapsed += endtimer_Elapsed;
            endtimer.Start();
        }

        void endtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.MixRtp("final");

            if (EndOfRtpStreamEvent != null)
                EndOfRtpStreamEvent(this, new EventArgs());
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.MixRtp(string.Empty);
        }

        public void Add(RecordInfo_t obj)
        {
            if (obj.size == 0)
            {
                endcount++;
            }

            if (endcount > 1)
            {
                if (timer != null)
                {
                    timer.Enabled = false;
                    timer.Stop();
                    timer.Close();
                    //timer = null;
                }

                if (endtimer != null)
                {
                    endtimer.Enabled = false;
                    endtimer.Stop();
                    endtimer.Close();
                    //endtimer = null;
                }

                this.MixRtp("final");

                if (EndOfRtpStreamEvent != null)
                    EndOfRtpStreamEvent(this, new EventArgs());

                return;
            }

            if (endtimer != null)
            {
                endtimer.Enabled = false;
                endtimer.Enabled = true;
            }

            if (obj.size == 0) return;

            if (obj.isExtension == 1)
            {
                lock (listIn)
                {
                    listIn.Add(new ReceivedRtp() { seq = obj.seq, size = obj.size, isExtension = obj.isExtension, ext = obj.extension, peer = obj.peer_number, buff = obj.voice });
                }
            }
            else
            {
                lock (listOut)
                {
                    listOut.Add(new ReceivedRtp() { seq = obj.seq, size = obj.size, isExtension = obj.isExtension, ext = obj.extension, peer = obj.peer_number, buff = obj.voice });
                }
            }
        }

        public void MixRtp(string check)
        {
            if (listIn == null || listOut == null)
                return;

            List<ReceivedRtp> linin = new List<ReceivedRtp>();
            List<ReceivedRtp> linout = new List<ReceivedRtp>();

            lock (listIn)
            {
                linin = listIn.ToList();
            }

            lock (listOut)
            {
                linout = listOut.ToList();
            }

            Com.Huen.Libs.SortRtpSeq sorting = new Com.Huen.Libs.SortRtpSeq();
            linin.Sort(sorting);
            linout.Sort(sorting);

            var itemIn = linin.FirstOrDefault();
            var itemOut = linout.FirstOrDefault();

            DelayedMil _delayedms = DelayedMil.same;
            if (itemIn == null || itemOut == null)
            {
                return;
            }
            else
            {
                byte[] mixedbytes = null;
                if ((itemIn.size - headersize) == 80 && (itemOut.size - headersize) == 160)
                {
                    _delayedms = DelayedMil.i80o160;

                    if (check.Equals("final"))
                    {
                        foreach (var item in linout)
                        {
                            mixedbytes = this.Mixing(linin, linout, item, _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < linout.Count * 0.8; i++)
                        {
                            mixedbytes = this.Mixing(linin, linout, linout[i], _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                }
                else if ((itemIn.size - headersize) == 160 && (itemOut.size - headersize) == 80)
                {
                    _delayedms = DelayedMil.i160o80;

                    if (check.Equals("final"))
                    {
                        foreach (var item in linin)
                        {
                            mixedbytes = this.Mixing(linin, linout, item, _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < linin.Count * 0.8; i++)
                        {
                            mixedbytes = this.Mixing(linin, linout, linin[i], _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                }
                else
                {
                    _delayedms = DelayedMil.same;

                    if (check.Equals("final"))
                    {
                        foreach (var item in linin)
                        {
                            mixedbytes = this.Mixing(linin, linout, item, _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < linin.Count * 0.8; i++)
                        {
                            mixedbytes = this.Mixing(linin, linout, linin[i], _delayedms);
                            this.WaveFileWriting(mixedbytes);
                        }
                    }
                }
            }
        }

        private int headersize = 12;
        private byte[] Mixing(List<ReceivedRtp> linin, List<ReceivedRtp> linout, ReceivedRtp item, DelayedMil _delayedms)
        {
            byte[] mixedbytes = null;

            if (_delayedms == DelayedMil.i80o160)
            {
                // item > out
                //List<ReceivedRtp> linein = new List<ReceivedRtp>();
                //linein = listIn.ToList();

                int seq = item.seq * 2;
                ReceivedRtp _item0 = linin.FirstOrDefault(x => x.seq == seq);
                ReceivedRtp _item1 = linin.FirstOrDefault(x => x.seq == seq + 1);

                if (_item0 == null)
                {
                    _item0 = new ReceivedRtp() { buff = new byte[332], seq = seq, size = 92, ext = item.ext, peer = item.peer };
                }

                if (_item1 == null)
                {
                    _item1 = new ReceivedRtp() { buff = new byte[332], seq = seq + 1, size = 92, ext = item.ext, peer = item.peer };
                }

                // item2 + tmpitem mix with item1 and write
                byte[] tmpbuff = new byte[332];
                Array.Copy(_item0.buff, 0, tmpbuff, 0, _item0.size);
                Array.Copy(_item1.buff, headersize, tmpbuff, _item0.size, (_item1.size - headersize));
                ReceivedRtp _itm = new ReceivedRtp() { buff = tmpbuff, size = (_item0.size + _item1.size - headersize) };
                this.RealMix(_itm, item, ref mixedbytes);

                lock (listIn)
                {
                    listIn.RemoveAll(x => x.seq == _item0.seq);
                    listIn.RemoveAll(x => x.seq == _item1.seq);
                }

                lock (listOut)
                {
                    listOut.RemoveAll(x => x.seq == item.seq);
                }
            }
            else if (_delayedms == DelayedMil.i160o80)
            {
                // item > in
                //List<ReceivedRtp> lineout = new List<ReceivedRtp>();
                //lineout = listOut.ToList();
                int seq = item.seq * 2;
                ReceivedRtp _item0 = linout.FirstOrDefault(x => x.seq == seq);
                ReceivedRtp _item1 = linout.FirstOrDefault(x => x.seq == seq + 1);

                if (_item0 == null)
                {
                    _item0 = new ReceivedRtp() { buff = new byte[332], seq = seq, size = 92, ext = item.ext, peer = item.peer };
                }

                if (_item1 == null)
                {
                    _item1 = new ReceivedRtp() { buff = new byte[332], seq = seq + 1, size = 92, ext = item.ext, peer = item.peer };
                }

                // item2 + tmpitem mix with item1 and write
                byte[] tmpbuff = new byte[332];
                Array.Copy(_item0.buff, 0, tmpbuff, 0, _item0.size);
                Array.Copy(_item1.buff, headersize, tmpbuff, _item0.size, (_item1.size - headersize));
                ReceivedRtp _itm = new ReceivedRtp() { buff = tmpbuff, size = (_item0.size + _item1.size - headersize) };
                this.RealMix(_itm, item, ref mixedbytes);

                lock (listIn)
                {
                    listIn.RemoveAll(x => x.seq == item.seq);
                }

                lock (listOut)
                {
                    listOut.RemoveAll(x => x.seq == _item0.seq);
                    listOut.RemoveAll(x => x.seq == _item1.seq);
                }
            }
            else
            {
                // same
                // item1 mix with item2 and write
                //List<ReceivedRtp> lineout = new List<ReceivedRtp>();
                //lineout = listOut.ToList();
                ReceivedRtp _item = linout.FirstOrDefault(x => x.seq == item.seq);
                if (_item == null)
                {
                    _item = new ReceivedRtp() { buff = new byte[332], seq = item.seq, size = item.size, ext = item.ext, peer = item.peer };
                }
                this.RealMix(item, _item, ref mixedbytes);

                lock (listIn)
                {
                    listIn.RemoveAll(x => x.seq == item.seq);
                }

                lock (listOut)
                {
                    listOut.RemoveAll(x => x.seq == _item.seq);
                }
            }

            return mixedbytes;
        }

        private void RealMix(ReceivedRtp item1, ReceivedRtp item2, ref byte[] buff)
        {
            if (item1 == null || item2 == null) return;

            if (item1.size == 0 || item2.size == 0) return;

            byte[] wavSrc1 = new byte[item1.size - headersize];
            byte[] wavSrc2 = new byte[item2.size - headersize];

            Array.Copy(item1.buff, headersize, wavSrc1, 0, (item1.size - headersize));
            Array.Copy(item2.buff, headersize, wavSrc2, 0, (item2.size - headersize));

            WaveMixerStream32 mixer = new WaveMixerStream32();
            //mixer.AutoStop = true;
            MemoryStream memstrem = new MemoryStream(wavSrc1);
            RawSourceWaveStream rawsrcstream = new RawSourceWaveStream(memstrem, this.codec);
            WaveFormatConversionStream conversionstream = new WaveFormatConversionStream(pcmFormat, rawsrcstream);
            WaveChannel32 channelstream = new WaveChannel32(conversionstream);
            mixer.AddInputStream(channelstream);

            memstrem = new MemoryStream(wavSrc2);
            rawsrcstream = new RawSourceWaveStream(memstrem, this.codec);
            conversionstream = new WaveFormatConversionStream(pcmFormat, rawsrcstream);
            channelstream = new WaveChannel32(conversionstream);
            mixer.AddInputStream(channelstream);
            mixer.Position = 0;

            Wave32To16Stream to16 = new Wave32To16Stream(mixer);
            var convStm = new WaveFormatConversionStream(pcmFormat, to16);
            byte[] mixedbytes = new byte[(int)convStm.Length];
            int chk = convStm.Read(mixedbytes, 0, (int)convStm.Length);

            memstrem.Close();
            rawsrcstream.Close();
            conversionstream.Close();
            channelstream.Close();

            convStm.Close(); convStm.Dispose(); convStm = null;
            to16.Close(); to16.Dispose(); to16 = null;
            mixer.Close(); mixer.Dispose(); mixer = null;

            buff = mixedbytes;
        }

        private void WaveFileWriting(byte[] buff)
        {
            if (buff == null) return;

            if (buff.Length == 0) return;

            using (MemoryStream memStm = new MemoryStream(buff))
            using (RawSourceWaveStream rawSrcStm = new RawSourceWaveStream(memStm, pcmFormat))
            {
                this.writer.Write(buff, 0, buff.Length);
                this.writer.Flush();
            }
        }

        public void Dispose()
        {
            if (this.writer != null)
            {
                writer.Close();
                writer = null;
            }

            if (listIn != null)
            {
                listIn.Clear(); listIn = null;
            }

            if (listOut != null)
            {
                listOut.Clear(); listOut = null;
            }

            if (timer != null)
                timer.Close();

            if (endtimer != null)
                endtimer.Close();
        }

        private enum DelayedMil
        {
            i80o160 = 0,
            i160o80 = 1,
            same = 2
        }
    }
}
