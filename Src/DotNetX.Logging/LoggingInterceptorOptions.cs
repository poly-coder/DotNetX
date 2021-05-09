using Microsoft.Extensions.Logging;
using System;

namespace DotNetX.Logging
{
    public class LoggingInterceptorOptions : ICloneable
    {
        public LogLevel StartLogLevel { get; set; } = LogLevel.Debug;
        public LogLevel DoneLogLevel { get; set; } = LogLevel.Information;
        public LogLevel NextLogLevel { get; set; } = LogLevel.Debug;
        public LogLevel ErrorLogLevel { get; set; } = LogLevel.Error;

        public string StartLogMessage { get; set; } = "{TypeName}.{MethodName}() | {Stage}";
        public string StartParametersLogMessage { get; set; } = "{TypeName}.{MethodName}({@Parameters}) | {Stage}";
        public string DoneLogMessage { get; set; } = "{TypeName}.{MethodName}() | {Stage}. Elapsed: ({Elapsed}ms)";
        public string DoneParametersLogMessage { get; set; } = "{TypeName}.{MethodName}({@Parameters}) | {Stage}. Elapsed: ({Elapsed}ms)";
        public string ResultLogMessage { get; set; } = "{TypeName}.{MethodName}() | {Stage} = ({@Result}). Elapsed: ({Elapsed}ms)";
        public string ResultParametersLogMessage { get; set; } = "{TypeName}.{MethodName}({@Parameters}) | {Stage} = ({@Result}). Elapsed: ({Elapsed}ms)";
        public string ErrorLogMessage { get; set; } = "{TypeName}.{MethodName}() | {Stage}. Elapsed: ({Elapsed}ms)";
        public string ErrorParametersLogMessage { get; set; } = "{TypeName}.{MethodName}({@Parameters}) | {Stage}. Elapsed: ({Elapsed}ms)";

        public string StartStage { get; set; } = "START";
        public string DoneStage { get; set; } = "DONE";
        public string CompleteStage { get; set; } = "COMPLETE";
        public string ResultStage { get; set; } = "RESULT";
        public string NextStage { get; set; } = "NEXT";
        public string ErrorStage { get; set; } = "ERROR";
        
        public string UnknownCategoryName { get; set; } = "UnknownType";
        public string UnknownTypeName { get; set; } = "UnknownType";

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public LoggingInterceptorOptions Clone()
        {
            var clone = (LoggingInterceptorOptions)this.MemberwiseClone();

            return clone;
        }
    }
}
