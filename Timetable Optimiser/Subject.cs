﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timetable_Optimiser
{
    class Subject : ICloneable
    {
        public Subject()
        {
            Classes = new List<Class>();
        }
        public string Code { get; set; }

        public Dictionary<string, int> ClassTypes
        {
            get
            {
                Dictionary<string, int> TypeInfo = new Dictionary<string, int>(); 
                foreach (Class c in Classes)
                {
                    if (!TypeInfo.ContainsKey(c.TypeCode))
                    {
                        TypeInfo[c.TypeCode] = 0;
                    }
                    TypeInfo[c.TypeCode]++;
                }

                return TypeInfo;
            }
        } 

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
