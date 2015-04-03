using System;
using System.Collections.Generic;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;

namespace HydraTest.CommandHandlers
{
    public class TestBase : IDisposable
    {
        private readonly IDisposable _context;
        private readonly Dictionary<string, Func<object>> _coreListProperties = new Dictionary<string, Func<object>>();
        private readonly Dictionary<string, Func<object>> _coreProperties = new Dictionary<string, Func<object>>();

        private readonly Dictionary<string, Func<object>> _transactionListProperties =
            new Dictionary<string, Func<object>>();

        private readonly Dictionary<string, Action<bool>> _transactionListValidators =
            new Dictionary<string, Action<bool>>();

        private readonly Dictionary<string, Func<object>> _transactionProperties =
            new Dictionary<string, Func<object>>();

        public TestBase()
        {
            _context = ShimsContext.Create();
            Transaction = new ShimSMTPTransaction();
            Core = new ShimSMTPCore();

            Transaction.ServerGet = () => Core;

            ShimSMTPTransaction.BehaveAsNotImplemented();
            ShimSMTPCore.BehaveAsNotImplemented();
        }

        protected ShimSMTPCore Core { get; set; }
        protected ShimSMTPTransaction Transaction { get; set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        protected void AddTransactionProperty<T>(string name, T value)
        {
            AddTransactionProperty(name, () => value);
        }

        protected void AddTransactionProperty<T>(string name, Func<T> value)
        {
            Transaction.GetPropertyOf1String(s =>
            {
                if (_transactionProperties.ContainsKey(s))
                {
                    return (T) _transactionProperties[s]();
                }
                throw new InvalidOperationException("The name is invalid.");
            });

            _transactionProperties.Add(name, () => value());
        }

        protected void AddCoreProperty<T>(string name, T value)
        {
            AddCoreProperty(name, () => value);
        }

        protected void AddCoreProperty<T>(string name, Func<T> value)
        {
            Core.GetPropertyOf1String(s =>
            {
                if (_coreProperties.ContainsKey(s))
                {
                    return (T) _coreProperties[s]();
                }
                throw new InvalidOperationException("The name is invalid.");
            });

            _coreProperties.Add(name, () => value());
        }

        protected void AddTransactionListProperty<T>(string name, IList<T> value, Action<bool> validator = null)
        {
            AddTransactionListProperty(name, () => value, validator);
        }

        protected void AddTransactionListProperty<T>(string name, Func<IList<T>> value, Action<bool> validator = null)
        {
            Transaction.GetListPropertyOf1StringBoolean((s, b) =>
            {
                if (_transactionListProperties.ContainsKey(s))
                {
                    if (_transactionListValidators.ContainsKey(s))
                    {
                        _transactionListValidators[s](b);
                    }

                    return (IList<T>) _transactionListProperties[s]();
                }
                throw new InvalidOperationException("The name is invalid.");
            });

            _transactionListProperties.Add(name, () => value());
            if (validator != null) _transactionListValidators.Add(name, validator);
        }

        protected void AddCoreListProperty<T>(string name, IList<T> value)
        {
            AddCoreListProperty(name, () => value);
        }

        protected void AddCoreListProperty<T>(string name, Func<IList<T>> value)
        {
            Core.GetListPropertyOf1String(s =>
            {
                if (_coreListProperties.ContainsKey(s))
                {
                    return (IList<T>) _coreListProperties[s]();
                }
                throw new InvalidOperationException("The name is invalid.");
            });

            _coreListProperties.Add(name, () => value());
        }
    }
}