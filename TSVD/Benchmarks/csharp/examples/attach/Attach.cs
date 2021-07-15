using System.Collections.Generic;
using System.Threading.Tasks;
using k8s;
using Microsoft.Coyote.Tasks;
using Task = Microsoft.Coyote.Tasks.Task;

namespace attach
{
    public class Attach
    {
        [Microsoft.Coyote.SystematicTesting.Test]
        public static void TestMethod()
        {
            var ws = new k8s.Tests.Mock.MockWebSocket();
            var demuxer = new StreamDemuxer(ws);

            var sentBuffer = new List<byte>();
            ws.MessageSent += (sender, args) => { sentBuffer.AddRange(args.Data.Buffer); };

            demuxer.Start();

            byte channelIndex = 12;

            var tasks = new List<Task>()
            {
                Task.Run( () => demuxer.GetStream(channelIndex, channelIndex)),
                Task.Run( () => demuxer.GetStream(channelIndex, channelIndex)),
            };

            Task.WaitAll(tasks.ToArray());
        }
    }
}
