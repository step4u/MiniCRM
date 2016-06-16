using System;
using System.Net;
using System.Net.Sockets;
using Com.Huen.Libs;
using System.Threading;
using Com.Huen.DataModel;
using System.Linq;

namespace Com.Huen.Sockets
{
    public delegate void RegSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void RegSuccessNatEventHandler(object obj, CommandMsg msg);
    public delegate void RegFailEventHandler(object obj, CommandMsg msg);
    public delegate void UnRegSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void MakeCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void MakeCallFailEventHandler(object obj, CommandMsg msg);

    public delegate void DropCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void DropCallFailEventHandler(object obj, CommandMsg msg);
    public delegate void PickupCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void PickupCallFailEventHandler(object obj, CommandMsg msg);
    
    public delegate void TransferCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void TransferCallFailEventHandler(object obj, CommandMsg msg);
    public delegate void HoldCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void HoldCallFailEventHandler(object obj, CommandMsg msg);
    public delegate void ActiveCallSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void ActiveCallFailEventHandler(object obj, CommandMsg msg);
    public delegate void EnableRecordRequestSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void EnableRecordRequestFailEventHandler(object obj, CommandMsg msg);
    public delegate void EnableRecordRequestOnNatSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void EnableRecordRequestOnNatFailEventHandler(object obj, CommandMsg msg);
    public delegate void DisableRecordRequestSuccessEventHandler(object obj, CommandMsg msg);
    public delegate void CallInvitingEventHandler(object obj, CommandMsg msg);
    public delegate void CallProceedingEventHandler(object obj, CommandMsg msg);
    public delegate void CallRingInEventHandler(object obj, CommandMsg msg);
    public delegate void CallRingOutEventHandler(object obj, CommandMsg msg);
    public delegate void CallConnectedEventHandler(object obj, CommandMsg msg);
    public delegate void CallFobiddenEventHandler(object obj, CommandMsg msg);
    public delegate void CallBusyEventHandler(object obj, CommandMsg msg);
    public delegate void CallTerminatedEventHandler(object obj, CommandMsg msg);
    public delegate void SmsSentCompletedEventHandler(object obj, sms_msg msg);
    public delegate void SmsSentInfoRequestedEventHandler(object obj, sms_msg msg);
    public delegate void SmsRecievedRequestedEventHandler(object obj, sms_msg msg);

    public delegate void SocketWasNotStartedEventHandler(object obj, Exception e);
    public delegate void ServerNotRespondEventHandler(object obj, SocketException e);
    public delegate void SocketErrorEventHandler(object obj, SocketException e);

    public class UdpCoupleMode
    {
        public event SocketWasNotStartedEventHandler SocketNotStarted;
        public event RegSuccessEventHandler RegSuccessEvent;
        public event RegSuccessNatEventHandler RegSuccessNatEvent;
        public event RegFailEventHandler RegFailEvent;
        public event UnRegSuccessEventHandler UnRegSuccessEvent;
        public event MakeCallSuccessEventHandler MakeCallSuccessEvent;
        public event MakeCallFailEventHandler MakeCallFailEvent;
        public event DropCallSuccessEventHandler DropCallSuccessEvent;
        public event DropCallFailEventHandler DropCallFailEvent;
        public event PickupCallSuccessEventHandler PickupCallSuccessEvent;
        public event PickupCallFailEventHandler PickupCallFailEvent;
        public event TransferCallSuccessEventHandler TransferCallSuccessEvent;
        public event TransferCallFailEventHandler TransferCallFailEvent;
        public event HoldCallSuccessEventHandler HoldCallSuccessEvent;
        public event HoldCallFailEventHandler HoldCallFailEvent;
        public event ActiveCallSuccessEventHandler ActiveCallSuccessEvent;
        public event ActiveCallFailEventHandler ActiveCallFailEvent;
        public event EnableRecordRequestSuccessEventHandler EnableRecordRequestSuccessEvent;
        public event EnableRecordRequestFailEventHandler EnableRecordRequestFailEvent;
        public event EnableRecordRequestOnNatSuccessEventHandler EnableRecordRequestOnNatSuccessEvent;
        public event EnableRecordRequestOnNatFailEventHandler EnableRecordRequestOnNatFailEvent;
        public event DisableRecordRequestSuccessEventHandler DisableRecordRequestSuccessEvent;
        public event CallInvitingEventHandler CallInvitingEvent;
        public event CallProceedingEventHandler CallProceedingEvent;
        public event CallRingInEventHandler CallRingInEvent;
        public event CallRingOutEventHandler CallRingOutEvent;
        public event CallConnectedEventHandler CallConnectedEvent;
        public event CallFobiddenEventHandler CallFobidenEvent;
        public event CallBusyEventHandler CallBusyEvent;
        public event CallTerminatedEventHandler CallTerminatedEvent;
        public event SmsSentCompletedEventHandler SmsSentCompletedEvent;
        public event SmsSentInfoRequestedEventHandler SmsSentInfoRequestedEvent;
        public event SmsRecievedRequestedEventHandler SmsRecievedRequestedEvent;
        
        public event ServerNotRespondEventHandler ServerNotRespondEvent;
        public event SocketErrorEventHandler SocketErrorEvent;

        private UdpClient client;
        private Thread sockthread;
        private int localport = 31001;

        private IPEndPoint ipep = null;
        public bool IsRegistered = false;

        private System.Timers.Timer regtimer = null;

        public UdpCoupleMode()
        {
            StartSocket();
        }

        private void StartSocket()
        {
            try
            {
                IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(CoupleModeInfo.pbxipaddress), CoupleModeInfo.pbxport);
                client = new UdpClient(localport);
                client.Connect(remoteEp);

                sockthread = new Thread(new ThreadStart(SendReceiveMessage));
                sockthread.IsBackground = true;
                sockthread.Start();

                this.Register();
            }
            catch (SocketException e)
            {
                util.WriteLog(e.ErrorCode, e.Message.ToString());

                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    if (ServerNotRespondEvent != null)
                        ServerNotRespondEvent(this, e);
                }
                else
                {
                    if (SocketErrorEvent != null)
                        SocketErrorEvent(this, e);
                }
            }
            catch (Exception e)
            {
                util.WriteLog(e.Message.ToString());
                if (SocketNotStarted != null)
                    SocketNotStarted(this, e);
            }
        }

        private void SendReceiveMessage()
        {
            try
            {
                // ipep = new IPEndPoint(IPAddress.Any, CoupleModeInfo.pbxport);
                ipep = new IPEndPoint(IPAddress.Parse(CoupleModeInfo.pbxipaddress), CoupleModeInfo.pbxport);

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
                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    if (ServerNotRespondEvent != null)
                        ServerNotRespondEvent(this, e);
                }
                else
                {
                    if (SocketErrorEvent != null)
                        SocketErrorEvent(this, e);
                }
            }
            //catch (Exception e)
            //{
            //    util.WriteLog(e.Message.ToString());
            //    if (SocketNotStarted != null)
            //        SocketNotStarted(this, e);
            //}
        }

        public void StopSocket()
        {
            try
            {
                if (sockthread == null) return;

                if (sockthread.IsAlive)
                {
                    sockthread.Abort();
                }

                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }
            catch (SocketException e)
            {
                util.WriteLog(e.Message.ToString());
            }
            catch (Exception e)
            {
                util.WriteLog(e.Message.ToString());
            }
        }

        public void RegTimerInit()
        {
            if (!this.IsRegistered)
            {
                regtimer = new System.Timers.Timer();
                regtimer.Interval = USRSTRUCTS.REGISTER_INTERVAL;
                regtimer.Elapsed += Regtimer_Elapsed;
                regtimer.Start();
            }
        }

        private void Regtimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Register();
        }

        public void Register()
        {
            CommandMsg _msg = GetCommandMsg(USRSTRUCTS.REGISTER_REQ);
            byte[] bytes = util.GetBytes(_msg);

            try
            {
                client.Send(bytes, bytes.Length);
            }
            catch (SocketException ex)
            {
                util.WriteLog(ex.ErrorCode, ex.Message.ToString());
            }
        }

        public void UnRegister()
        {
            if (client == null) return;

            CommandMsg _msg = GetCommandMsg(USRSTRUCTS.UNREGISTER_REQ);
            byte[] bytes = util.GetBytes(_msg);

            try
            {
                client.Send(bytes, bytes.Length);
            }
            catch (SocketException ex)
            {
                util.WriteLog(ex.ErrorCode, ex.Message.ToString());
            }
        }

        // Make a Call
        public void MakeCall(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.MAKE_CALL_REQ, number);
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

        public void DropCall(CallList call)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.DROP_CALL_REQ, call);
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

        public void TransferCall(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.TRANSFER_CALL_REQ, number);
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

        public void PickupCall(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.PICKUP_CALL_REQ, number);
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

        public void HoldCall(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.HOLD_CALL_REQ, number);
            // CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.HOLD_CALL_REQ);
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

        public void ActiveCall(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.ACTIVE_CALL_REQ, number);
            // CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.ACTIVE_CALL_REQ);
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

        public void RecordStartRequest(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.ENABLE_CALL_RECORD_REQ, number);
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

        public void RecordStopRequest(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.DISABLE_CALL_RECORD_REQ, number);
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

        public void RecordStartRequestOnNat(string number)
        {
            CommandMsg msg = this.GetCommandMsg(USRSTRUCTS.ENABLE_NAT_CALL_RECORD_REQ, number);
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

        // Receive Sms
        public void ResponseSmsReceivedRequested(sms_msg msg)
        {
            sms_msg sendSmsMsg = GetSmsMessage(USRSTRUCTS.SMS_RECV_RES, string.Empty, msg.to_ext, msg.senderphone, msg.senderphone, string.Empty);
            byte[] bytes = util.GetBytes(sendSmsMsg);
            client.Send(bytes, bytes.Length);
        }

        // Send Sms
        public void SendSms(Customer customer, string smsmsg, string sender)
        {
            try
            {
                sms_msg msg = this.GetSmsMessage(USRSTRUCTS.SMS_SEND_REQ, smsmsg, sender, customer.Cellular, string.Empty, string.Empty);
                byte[] bytes = util.GetBytes(msg);

                client.Send(bytes, bytes.Length);
            }
            catch (SocketException ex)
            {
                util.WriteLog(ex.ErrorCode, ex.Message.ToString());
            }

            // Thread.Sleep(100);
        }

        public void ResponseSmsInfoRequested(sms_msg msg)
        {
            sms_msg sendSmsMsg = GetSmsMessage(USRSTRUCTS.SMS_INFO_RES, string.Empty, msg.from_ext, msg.receiverphones, msg.to_ext, string.Empty);
            byte[] bytes = util.GetBytes(sendSmsMsg);
            client.Send(bytes, bytes.Length);
        }

        private void DoItFromSocketMessage(byte[] buffer)
        {
            CommandMsg rcvMsg = util.GetObject<CommandMsg>(buffer);

            util.WriteStructVal(rcvMsg);

            if (rcvMsg.cmd == USRSTRUCTS.SMS_SEND_REQ
                || rcvMsg.cmd == USRSTRUCTS.SMS_INFO_REQ
                || rcvMsg.cmd == USRSTRUCTS.SMS_RECV_REQ
                || rcvMsg.cmd == USRSTRUCTS.SMS_SEND_RES
                || rcvMsg.cmd == USRSTRUCTS.SMS_INFO_RES
                || rcvMsg.cmd == USRSTRUCTS.SMS_RECV_RES)
            {
                DoItForSMS(buffer);
                return;
            }

            switch (rcvMsg.cmd)
            {
                case USRSTRUCTS.REGISTER_RES:
                    if (rcvMsg.status == USRSTRUCTS.STATUS_SUCCESS)
                    {
                        // 성공
                        if (RegSuccessEvent != null)
                            RegSuccessEvent(this, rcvMsg);
                    }
                    else if (rcvMsg.status == USRSTRUCTS.STATUS_FAIL)
                    {
                        // 실패
                        if (RegFailEvent != null)
                            RegFailEvent(this, rcvMsg);
                    }
                    else
                    {
                        // 성공 NAT
                        if (RegSuccessNatEvent != null)
                            RegSuccessNatEvent(this, rcvMsg);
                    }
                    break;
                case USRSTRUCTS.UNREGISTER_RES:
                    if (UnRegSuccessEvent != null)
                        UnRegSuccessEvent(this, rcvMsg);
                    break;
                case USRSTRUCTS.MAKE_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (MakeCallSuccessEvent != null)
                                MakeCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (MakeCallFailEvent != null)
                                MakeCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.DROP_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (DropCallSuccessEvent != null)
                                DropCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (DropCallFailEvent != null)
                                DropCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.PICKUP_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (PickupCallSuccessEvent != null)
                                PickupCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (PickupCallFailEvent != null)
                                PickupCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.TRANSFER_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (TransferCallSuccessEvent != null)
                                TransferCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (TransferCallFailEvent != null)
                                TransferCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.HOLD_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (HoldCallSuccessEvent != null)
                                HoldCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (HoldCallFailEvent != null)
                                HoldCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.ACTIVE_CALL_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (ActiveCallSuccessEvent != null)
                                ActiveCallSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (ActiveCallFailEvent != null)
                                ActiveCallFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.ENABLE_CALL_RECORD_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (EnableRecordRequestSuccessEvent != null)
                                EnableRecordRequestSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (EnableRecordRequestFailEvent != null)
                                EnableRecordRequestFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.ENABLE_NAT_CALL_RECORD_RES:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_SUCCESS:
                            if (EnableRecordRequestOnNatSuccessEvent != null)
                                EnableRecordRequestOnNatSuccessEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_FAIL:
                        default:
                            if (EnableRecordRequestOnNatFailEvent != null)
                                EnableRecordRequestOnNatFailEvent(this, rcvMsg);
                            break;
                    }
                    break;
                case USRSTRUCTS.DISABLE_CALL_RECORD_RES:
                    if (DisableRecordRequestSuccessEvent != null)
                        DisableRecordRequestSuccessEvent(this, rcvMsg);
                    break;
                case USRSTRUCTS.CALL_STATUS:
                    switch (rcvMsg.status)
                    {
                        case USRSTRUCTS.STATUS_CALL_INVITING:
                            if (CallInvitingEvent != null)
                                CallInvitingEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_PROCEEDING:
                            if (CallProceedingEvent != null)
                                CallProceedingEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_RINGING:
                            if (rcvMsg.direct == USRSTRUCTS.DIRECT_INCOMING)
                            {
                                if (CallRingInEvent != null)
                                    CallRingInEvent(this, rcvMsg);
                            }
                            else if (rcvMsg.direct == USRSTRUCTS.DIRECT_OUTGOING)
                            {
                                if (CallRingOutEvent != null)
                                    CallRingOutEvent(this, rcvMsg);
                            }
                            break;
                        case USRSTRUCTS.STATUS_CALL_CONNECTED:
                            if (CallConnectedEvent != null)
                                CallConnectedEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_NO_ANSWER:
                            if (CallConnectedEvent != null)
                                CallConnectedEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_FORBIDDEN:
                            if (CallFobidenEvent != null)
                                CallFobidenEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_BUSY:
                            if (CallBusyEvent != null)
                                CallBusyEvent(this, rcvMsg);
                            break;
                        case USRSTRUCTS.STATUS_CALL_TERMINATED:
                            if (CallTerminatedEvent != null)
                                CallTerminatedEvent(this, rcvMsg);
                            break;
                    }
                    break;
                default:
                    
                    break;
            }
        }

        private void DoItForSMS(byte[] buffer)
        {
            sms_msg msg = util.GetObject<sms_msg>(buffer);

            switch (msg.cmd)
            {
                case USRSTRUCTS.SMS_SEND_RES:
                    if (SmsSentCompletedEvent != null)
                        SmsSentCompletedEvent(this, msg);
                    break;
                case USRSTRUCTS.SMS_INFO_REQ:
                    if (SmsSentInfoRequestedEvent != null)
                        SmsSentInfoRequestedEvent(this, msg);
                    break;
                case USRSTRUCTS.SMS_RECV_REQ:
                    if (SmsRecievedRequestedEvent != null)
                        SmsRecievedRequestedEvent(this, msg);
                    break;
                default:
                    break;
            }

            // sendmsg = GetSmsMessage(USRSTRUCTS.SMS_INFO_RES, string.Empty, msg.from_ext, msg.receiverphones, msg.to_ext, string.Empty);

            // byte[] bytes = util.GetBytes(sendmsg);
            // client.Send(bytes, bytes.Length);

            // util.WriteLog(string.Format("cmd : {0}, status : {1}, receiverphones : {2}, toext : {3}, fromext : {4}", msg.cmd, msg.status, msg.receiverphones, msg.to_ext, msg.from_ext));
        }

        #region 메시지 생성 메소드
        public CommandMsg GetCommandMsg(byte st)
        {
            CommandMsg msg = new CommandMsg();

            switch (st)
            {
                case USRSTRUCTS.REGISTER_REQ:
                    msg.cmd = USRSTRUCTS.REGISTER_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    //msg.to_ext = to_ext;
                    msg.userid = string.Empty;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.UNREGISTER_REQ:
                    msg.cmd = USRSTRUCTS.UNREGISTER_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    //msg.to_ext = to_ext;
                    msg.userid = string.Empty;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.DROP_CALL_REQ:
                    msg.cmd = USRSTRUCTS.DROP_CALL_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.PICKUP_CALL_REQ:
                    msg.cmd = USRSTRUCTS.PICKUP_CALL_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.TRANSFER_CALL_REQ:
                    msg.cmd = USRSTRUCTS.TRANSFER_CALL_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.HOLD_CALL_REQ:
                    msg.cmd = USRSTRUCTS.HOLD_CALL_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.ACTIVE_CALL_REQ:
                    msg.cmd = USRSTRUCTS.ACTIVE_CALL_REQ;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    break;
            }

            return msg;
        }

        public CommandMsg GetCommandMsg(byte st, string toext)
        {
            CommandMsg msg = new CommandMsg() { cmd = st };

            switch (st)
            {
                case USRSTRUCTS.REGISTER_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.userid = string.Empty;
                    break;
                case USRSTRUCTS.UNREGISTER_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.userid = string.Empty;
                    break;
                case USRSTRUCTS.MAKE_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.userid = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.DROP_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    break;
                case USRSTRUCTS.PICKUP_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_INCOMING;
                    msg.from_ext = CoupleModeInfo.userid; 
                    msg.to_ext = toext;
                    // msg.userid = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.TRANSFER_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.HOLD_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.ACTIVE_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.ENABLE_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
                case USRSTRUCTS.ENABLE_NAT_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
                case USRSTRUCTS.DISABLE_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = toext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
            }

            return msg;
        }

        public CommandMsg GetCommandMsg(byte st, CallList call)
        {
            CommandMsg msg = new CommandMsg() { cmd = st };

            switch (st)
            {
                case USRSTRUCTS.REGISTER_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.userid = string.Empty;
                    break;
                case USRSTRUCTS.UNREGISTER_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.userid = string.Empty;
                    break;
                case USRSTRUCTS.MAKE_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.userid = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.DROP_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    break;
                case USRSTRUCTS.PICKUP_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_INCOMING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    // msg.userid = CoupleModeInfo.userid;
                    break;
                case USRSTRUCTS.TRANSFER_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.HOLD_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.ACTIVE_CALL_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.userid = CoupleModeInfo.userid;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = localport;
                    break;
                case USRSTRUCTS.ENABLE_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
                case USRSTRUCTS.ENABLE_NAT_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
                case USRSTRUCTS.DISABLE_CALL_RECORD_REQ:
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.to_ext = call.to_ext;
                    msg.ip = util.IpAddress2Int(util.DoGetHostEntry());
                    msg.port = 21010;
                    break;
            }

            return msg;
        }

        public sms_msg GetSmsMessage(byte st, string strmsg, string sender, string receiver, string toext, string reservtime)
        {
            sms_msg msg = new sms_msg();

            switch (st)
            {
                case USRSTRUCTS.SMS_SEND_REQ:
                    msg.cmd = st;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = CoupleModeInfo.userid;
                    msg.message = strmsg;
                    msg.senderphone = sender;
                    msg.receiverphones = receiver;
                    msg.reservetime = reservtime;
                    break;
                case USRSTRUCTS.SMS_INFO_RES:
                    msg.cmd = st;
                    msg.direct = USRSTRUCTS.DIRECT_OUTGOING;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.from_ext = sender;
                    msg.receiverphones = receiver;
                    msg.to_ext = toext;
                    break;
                case USRSTRUCTS.SMS_RECV_RES:
                    msg.cmd = st;
                    msg.direct = USRSTRUCTS.DIRECT_INCOMING;
                    msg.type = USRSTRUCTS.TYPE_COUPLEPHONE;
                    msg.status = USRSTRUCTS.STATUS_SMS_SUCCESS;
                    msg.from_ext = sender;
                    msg.senderphone = sender;
                    msg.receiverphones = receiver;
                    msg.to_ext = receiver;
                    msg.userid = receiver;
                    break;
            }

            return msg;
        }
        #endregion 메시지 생성 메소드

    }
}
