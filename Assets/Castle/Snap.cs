using UnityEngine;

public class Snap {

    public Vector3 snappingPoint;
    public Vector3 snappingDirection;
    public float snappingPriority;
    public SnappingClass cls;

    public Snap(Vector3 origin, Vector3 direction, float priority, SnappingClass cls) {
        this.snappingPoint = origin;
        this.snappingDirection = direction;
        this.snappingPriority = priority;
        this.cls = cls;
    }

    public enum SnappingClass {
        POINT, EDGE
    }
    
}