﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CPI.Providers
{
    public sealed class LockProvider
    {
        private readonly Dictionary<Int32, Int32> _lockDic;
        private readonly ReaderWriterLockSlim _rwLock;

        public LockProvider(Int32 capacity = 1000)
        {
            if (capacity <= 0)
            {
                capacity = 1000;
            }

            _lockDic = new Dictionary<Int32, Int32>(capacity);
            _rwLock = new ReaderWriterLockSlim();
        }

        public Boolean Exists(Int32 key)
        {
            try
            {
                _rwLock.EnterReadLock();
                return _lockDic.TryGetValue(key, out _);
            }
            finally
            {
                if (_rwLock.IsReadLockHeld) { _rwLock.ExitReadLock(); }
            }
        }

        public Boolean Lock(Int32 key)
        {
            try
            {
                _rwLock.EnterWriteLock();

                if (_lockDic.TryGetValue(key, out Int32 value))
                {
                    return false;
                }

                _lockDic[key] = 1;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (_rwLock.IsWriteLockHeld)
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }

        public void UnLock(Int32 key)
        {
            try
            {
                _rwLock.EnterWriteLock();
                _lockDic.Remove(key);
            }
            catch { }
            finally
            {
                if (_rwLock.IsWriteLockHeld)
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }

        public IDisposable CreateScope(Int32 key)
        {
            return new LockScope(this, key);
        }

        internal class LockScope : IDisposable, IEquatable<LockScope>
        {
            private readonly LockProvider _provider;
            private readonly Int32 _key;

            public LockScope(LockProvider provider, Int32 key)
            {
                _provider = provider;
                _key = key;

                if (!_provider.Exists(_key))
                {
                    _provider.Lock(_key);
                }
            }

            public void Dispose()
            {
                if (_provider.Exists(_key))
                {
                    _provider.UnLock(_key);
                }
            }

            public Boolean Equals(LockScope other)
            {
                return this._key == other._key;
            }
        }
    }
}
