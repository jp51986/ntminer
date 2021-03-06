﻿using NTMiner.ServiceContracts.DataObjects;
using System;
using System.Collections.Generic;

namespace NTMiner.Data {
    public interface IClientSet {
        int OnlineCount { get; }

        int MiningCount { get; }

        int CountMainCoinOnline(string coinCode);

        int CountDualCoinOnline(string coinCode);

        int CountMainCoinMining(string coinCode);

        int CountDualCoinMining(string coinCode);

        void Add(ClientData clientData);

        void UpdateClient(Guid clientId, string propertyName, object value);

        List<ClientData> QueryClients(
            int pageIndex,
            int pageSize,
            Guid? mineWorkId,
            string minerIp,
            string minerName,
            MineStatus mineState,
            string mainCoin,
            string mainCoinPool,
            string mainCoinWallet,
            string dualCoin,
            string dualCoinPool,
            string dualCoinWallet,
            out int total);

        List<ClientData> LoadClients(IEnumerable<Guid> clientIds);

        ClientData LoadClient(Guid clientId);
    }
}
