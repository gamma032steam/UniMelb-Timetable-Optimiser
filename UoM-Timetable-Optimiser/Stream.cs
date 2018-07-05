using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Timetable_Optimiser
{
    [Serializable]
    public class StreamContainer
    {
        public List<Stream> Streams;

        public StreamContainer()
        {
            Streams = new List<Stream>();
        }
        private bool StreamExists(int number)
        {
            return Streams.Any(x => x.StreamNumber == number);
        }
        public void AddStreamClass(char streamType, int streamNumber, Class c)
        {
            c.Type = Class.ClassType.Stream;
            if (!StreamExists(streamNumber))
            {
                Streams.Add(new Stream(streamType, streamNumber, c));
            }
            else
            {
                Streams.Where(x => x.StreamNumber == streamNumber).ToList()[0].Classes.Add(c);
            }
        }
    }
    [Serializable]
    public struct Stream
    {
        public int StreamNumber;
        public List<Class> Classes;
        public char StreamType { get; }
        public Stream(char streamType, int streamNumber, Class firstClass = null)
        {
            StreamType = streamType;
            StreamNumber = streamNumber;
            this.Classes = new List<Class> { firstClass };
        }
    }
}
