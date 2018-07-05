using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Timetable_Optimiser
{
    [Serializable]
    public class Subject : ICloneable
    {
        public Subject(string subjectCode, Dictionary<char, StreamContainer> streamContainer = null)
        {
            Code = subjectCode;
            Classes = new List<Class>();
            StreamContainers = streamContainer ?? new Dictionary<char, StreamContainer>();
        }

        public string Code { get; }

        public Dictionary<string, int> ClassCodeTypes
        {
            get
            {
                Dictionary<string, int> typeInfo = new Dictionary<string, int>();
                foreach (Class c in Classes)
                {
                    if (!typeInfo.ContainsKey(c.ClassCode))
                    {
                        typeInfo[c.ClassCode] = 0;
                    }
                    typeInfo[c.ClassCode]++;
                }

                return typeInfo;
            }
        }

        public Dictionary<char, StreamContainer> StreamContainers { get; set; }

        public List<Class> Classes { get; private set; }

        public void AddClass(Class c)
        {
            c.SubjectCode = Code;
            Classes.Add(c);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
