using UnityEngine;
using System.Linq;

public class OutlineItem{

    #region events
    public delegate void Update();
    public event Update OnUpdate;
    #endregion

    private Vector2 start;
    private Vector2 end;

    public Vector2 Start {
        get {
            return start;
        }

        set {
            start = value;
            if(OnUpdate!=null)
                OnUpdate();
        }
    }

    public Vector2 End {
        get {
            return end;
        }

        set {
            end = value;
            if(OnUpdate != null)
                OnUpdate();
        }
    }

    public OutlineItem(Vector2 start, Vector2 end) {
        this.start = start;
        this.end = end;
    }

    public Vector2[] getDefiningPoints() {
        return new Vector2[] { end };
    }

}

