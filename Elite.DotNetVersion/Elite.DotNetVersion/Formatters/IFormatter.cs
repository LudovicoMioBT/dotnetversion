using System;
using System.IO;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Formatters
{
    interface IFormatter : IDisposable
    {
        Task WriteAsync(TextWriter writer, object content);
    }
}
