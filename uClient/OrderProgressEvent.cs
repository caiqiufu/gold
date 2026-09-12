using System;
namespace uClient.Comm
{
    public class OrderProgressEvent
    {
        //
        // Summary:
        //     Temporary ID. Useful until server assign ticket number.
        public int TempID;
        //
        // Summary:
        //     Stage of order processing by server.
        public ProgressType Type;
        //
        // Summary:
        //     Opened/closed order.
        public Position postion;
        //
        // Summary:
        //     Exception during processing of the order. Could be ServerException or RequoteException.
        public Exception Exception;

        public DateTime SendTime;

        public DateTime ReceiveTime;

        public string result;
        public string toString()
        {
            return string.Format("TempID：{0} Type:{1} ReceiveTime:{2} result:{3}", TempID, Type, ReceiveTime, result);
        }
    }
    //
    // Summary:
    //     Stage of order processing by server.
    public enum ProgressType
    {
        //
        // Summary:
        //     Order was rejected.
        Rejected = 0,
        //
        // Summary:
        //     Order was accepted by server.
        Accepted = 1,
        //
        // Summary:
        //     Server started to execute the order.
        InProcess = 2,
        //
        // Summary:
        //     Order was opened.
        Opened = 3,
        //
        // Summary:
        //     Order was closed.
        Closed = 4,
        //
        // Summary:
        //     Order was modified.
        Modified = 5,
        //
        // Summary:
        //     Pending order was deleted.
        PendingDeleted = 6,
        //
        // Summary:
        //     Closed of pair of opposite orders.
        ClosedBy = 7,
        //
        // Summary:
        //     Closed of multiple orders.
        MultipleClosedBy = 8,
        //
        // Summary:
        //     Trade timeout.
        Timeout = 9,
        //
        // Summary:
        //     Price data.
        Price = 10,
        //
        // Summary:
        //     Exception.
        Exception = 11
    }
}
