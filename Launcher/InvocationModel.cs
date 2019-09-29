﻿using System.Collections.Generic;

namespace Launcher
{
    public class FunctionCall
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        public HashSet<FunctionCall> Children { get; } = new HashSet<FunctionCall>();
        public bool Recursive { get; internal set; }
        public bool TailCall { get; internal set; }

        public override bool Equals(object obj)
        {
            return Name == ((FunctionCall)obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class InvocationModel
        {
    


        public InvocationModel(List<FunctionCall> model)
            {
                AllFunctions = model;
            }

            public List<FunctionCall> AllFunctions { get; }
        }
    }