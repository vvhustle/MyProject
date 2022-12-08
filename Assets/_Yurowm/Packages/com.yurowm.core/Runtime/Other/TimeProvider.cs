using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yurowm {
    public interface ITimeProvider {
        bool HasTime {get;}
        DateTime Now {get;}
        DateTime UTCNow {get;}
    }
    
    public class SystemTimeProvider : ITimeProvider {
        public bool HasTime => true;
        public DateTime Now => DateTime.Now;
        public DateTime UTCNow => DateTime.UtcNow;
    }
}