using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public abstract class SemiInstancedProperty : ItemProperty //Shared part
{
    public abstract class InstancedData<T> : ItemInstanceProperty where T : SemiInstancedProperty //Instanced part
    {
        protected T shared { get; private set; }

        protected InstancedData(T shared)
        {
            this.shared = shared;
        }
    }
}
