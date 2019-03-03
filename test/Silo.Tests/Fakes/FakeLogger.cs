﻿using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Silo.Tests.Fakes
{
    public class FakeLogger : ILogger
    {
        private readonly string _categoryName;

        public FakeLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new FakeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Debug.Print($"[{logLevel}]:[{eventId}]:[{_categoryName}]:[{formatter(state, exception)}]");
        }
    }
}