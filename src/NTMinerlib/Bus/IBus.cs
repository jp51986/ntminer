﻿
namespace NTMiner.Bus {
    using System;
    using System.Collections.Generic;

    public interface IBus : IDisposable {
        void Publish<TMessage>(TMessage message);

        void Publish<TMessage>(IEnumerable<TMessage> messages);

        void Commit();

        void Clear();
    }
}
