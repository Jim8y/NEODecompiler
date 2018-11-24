using System;
using System.Collections.Generic;
using System.Text;

namespace NEODisassembler
{
    public class OpAndCall
    {
        public int pop;
        public int push;
        public OpAndCall(int po, int pu)
        {
            this.pop = po;
            this.push = pu;
        }
    }
    public class OpCall
    {
        public static Dictionary<int, OpAndCall> opCall = new Dictionary<int, OpAndCall>();
        OpCall()
        {

        }
    }
    public class OpAndSysCall
    {
        //      name input output
        public static Dictionary<string, OpAndCall> sysCall = new Dictionary<string, OpAndCall>();
        OpAndSysCall()
        {
            sysCall.Add("Neo.Runtime.GetTrigger", new OpAndCall(0, 1));

            sysCall.Add("Neo.Runtime.CheckWitness", new OpAndCall(0, 1));
            sysCall.Add("Neo.Runtime.Notify", new OpAndCall(1, 0));
            sysCall.Add("Neo.Runtime.Log", new OpAndCall(1, 0));
            sysCall.Add("Neo.Runtime.GetTime", new OpAndCall(0, 1));
            sysCall.Add("Neo.Runtime.Serialize", new OpAndCall(1, 1));
            sysCall.Add("Neo.Runtime.Deserialize", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetHeight", new OpAndCall(0, 1));
            sysCall.Add("Neo.Blockchain.GetHeader", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetBlock", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetTransaction", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetTransactionHeight", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetAccount", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetValidators", new OpAndCall(0, 1));
            sysCall.Add("Neo.Blockchain.GetAsset", new OpAndCall(1, 1));
            sysCall.Add("Neo.Blockchain.GetContract", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetIndex", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetVersion", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetPrevHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetMerkleRoot", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetTimestamp", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetConsensusData", new OpAndCall(1, 1));
            sysCall.Add("Neo.Header.GetNextConsensus", new OpAndCall(1, 1));
            sysCall.Add("Neo.Block.GetTransactionCount", new OpAndCall(1, 1));
            sysCall.Add("Neo.Block.GetTransactions", new OpAndCall(1, 1));
            sysCall.Add("Neo.Block.GetTransaction", new OpAndCall(2, 1));
            sysCall.Add("Neo.Transaction.GetHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetType", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetAttributes", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetInputs", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetOutputs", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetReferences", new OpAndCall(1, 1));
            sysCall.Add("Neo.Transaction.GetUnspentCoins", new OpAndCall(1, 1));
            sysCall.Add("Neo.InvocationTransaction.GetScript", new OpAndCall(1, 1));
            sysCall.Add("Neo.Attribute.GetUsage", new OpAndCall(1, 1));
            sysCall.Add("Neo.Attribute.GetData", new OpAndCall(1, 1));
            sysCall.Add("Neo.Input.GetHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Input.GetIndex", new OpAndCall(1, 1));
            sysCall.Add("Neo.Output.GetAssetId", new OpAndCall(1, 1));
            sysCall.Add("Neo.Output.GetValue", new OpAndCall(1, 1));
            sysCall.Add("Neo.Output.GetScriptHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Account.GetScriptHash", new OpAndCall(1, 1));
            sysCall.Add("Neo.Account.GetVotes", new OpAndCall(1, 1));
            sysCall.Add("Neo.Account.GetBalance", new OpAndCall(2, 1));
            sysCall.Add("Neo.Asset.GetAssetId", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetAssetType", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetAmount", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetAvailable", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetPrecision", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetOwner", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetAdmin", new OpAndCall(1, 1));
            sysCall.Add("Neo.Asset.GetIssuer", new OpAndCall(1, 1));
            sysCall.Add("Neo.Contract.GetScript", new OpAndCall(1, 1));
            sysCall.Add("Neo.Contract.IsPayable", new OpAndCall(1, 1));
            sysCall.Add("Neo.Storage.GetContext", new OpAndCall(0, 1));
            sysCall.Add("Neo.Storage.GetReadOnlyContext", new OpAndCall(0, 1));
            sysCall.Add("Neo.Storage.Get", new OpAndCall(2, 1));
            sysCall.Add("Neo.Storage.Find", new OpAndCall(2, 1));
            sysCall.Add("Neo.StorageContext.AsReadOnly", new OpAndCall(1, 1));
            sysCall.Add("Neo.Enumerator.Create", new OpAndCall(1, 1));
            sysCall.Add("Neo.Enumerator.Next", new OpAndCall(1, 1));
            sysCall.Add("Neo.Enumerator.Value", new OpAndCall(1, 1));
            sysCall.Add("Neo.Enumerator.Concat", new OpAndCall(2, 1));
            sysCall.Add("Neo.Iterator.Create", new OpAndCall(1, 1));
            sysCall.Add("Neo.Iterator.Key", new OpAndCall(1, 1));
            sysCall.Add("Neo.Iterator.Keys", new OpAndCall(1, 1));
            sysCall.Add("Neo.Iterator.Values", new OpAndCall(1, 1));
            sysCall.Add("Neo.Iterator.Next", new OpAndCall(1, 1));
            sysCall.Add("Neo.Iterator.Value", new OpAndCall(1, 1));
            sysCall.Add("System.ExecutionEngine.GetScriptContainer", new OpAndCall(0, 1));
            sysCall.Add("System.ExecutionEngine.GetExecutingScriptHash", new OpAndCall(0, 1));
            sysCall.Add("System.ExecutionEngine.GetCallingScriptHash", new OpAndCall(0, 1));
            sysCall.Add("System.ExecutionEngine.GetEntryScriptHash", new OpAndCall(0, 1));
        }

    }
}
