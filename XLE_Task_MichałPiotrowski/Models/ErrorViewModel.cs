using System;

namespace XLE_Task_MichałPiotrowski.Models {
    public class ErrorViewModel {
        public string RequestId { get; set; }
        public string Message { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
