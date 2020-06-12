// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Transport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Zxw.Framework.NetCore.Transaction
{
    public class DefaultCapDbTransaction : CapTransactionBase
    {
        private readonly DiagnosticProcessorObserver _diagnosticProcessor;

        public DefaultCapDbTransaction(
            IDispatcher dispatcher) : base(dispatcher)
        {
            _diagnosticProcessor = new DiagnosticProcessorObserver(dispatcher);
        }

        protected override void AddToSent(MediumMessage msg)
        {
            if (DbTransaction is NoopTransaction)
            {
                base.AddToSent(msg);
                return;
            }

            var dbTransaction = DbTransaction as IDbTransaction;
            if (dbTransaction == null)
            {
                if (DbTransaction is IDbContextTransaction dbContextTransaction)
                    dbTransaction = dbContextTransaction.GetDbTransaction();

                if (dbTransaction == null) throw new ArgumentNullException(nameof(DbTransaction));
            }

            var transactionKey = ((SqlConnection)dbTransaction.Connection).ClientConnectionId;
            if (_diagnosticProcessor.BufferList.TryGetValue(transactionKey, out var list))
            {
                list.Add(msg);
            }
            else
            {
                var msgList = new List<MediumMessage>(1) { msg };
                _diagnosticProcessor.BufferList.TryAdd(transactionKey, msgList);
            }
        }

        public override void Commit()
        {
            switch (DbTransaction)
            {
                case NoopTransaction _:
                    Flush();
                    break;
                case IDbTransaction dbTransaction:
                    dbTransaction.Commit();
                    break;
                case IDbContextTransaction dbContextTransaction:
                    dbContextTransaction.Commit();
                    break;
            }
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            switch (DbTransaction)
            {
                case NoopTransaction _:
                    Flush();
                    break;
                case IDbTransaction dbTransaction:
                    dbTransaction.Commit();
                    break;
                case IDbContextTransaction dbContextTransaction:
                    await dbContextTransaction.CommitAsync(cancellationToken);
                    break;
            }
        }

        public override void Rollback()
        {
            switch (DbTransaction)
            {
                case IDbTransaction dbTransaction:
                    dbTransaction.Rollback();
                    break;
                case IDbContextTransaction dbContextTransaction:
                    dbContextTransaction.Rollback();
                    break;
            }
        }

        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            switch (DbTransaction)
            {
                case IDbTransaction dbTransaction:
                    dbTransaction.Rollback();
                    break;
                case IDbContextTransaction dbContextTransaction:
                    await dbContextTransaction.RollbackAsync(cancellationToken);
                    break;
            }
        }

        public override void Dispose()
        {
            switch (DbTransaction)
            {
                case IDbTransaction dbTransaction:
                    dbTransaction.Dispose();
                    break;
                case IDbContextTransaction dbContextTransaction:
                    dbContextTransaction.Dispose();
                    break;
            }

            DbTransaction = null;
        }
    }
}