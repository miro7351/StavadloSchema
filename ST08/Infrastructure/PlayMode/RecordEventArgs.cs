using System;

namespace Stavadlo22.Infrastructure.PlayMode
{ 
    //MH:
    /// <summary>
    /// argument pre event
    /// </summary>
    public class RecordEventArgs : EventArgs
    {
        public string RecievedDate { get; set; }
        public string RecordNumber { get; set; }
        public string Message { get; set; }
    }
}
