using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.Collections;

public class Outline : IEnumerable<OutlineItem>{

    #region events
    public delegate void Update();
    public event Update OnShapeChange;
    public event Update OnStateChange;
    #endregion

    private bool complete;
    private bool enclosed;

    private float height;
    private List<OutlineItem> shapes = new List<OutlineItem>();
    private Bounds bounds;

    private float extrusion = 0;
    private bool bothSideExtrusion = true;

    public static float subPointDistance = 1;

    #region properties

    public bool Complete {
        get {
            return complete;
        }

        set {
            complete = value;
            if(OnStateChange!=null)
                OnStateChange();
        }
    }
    public bool Enclosed {
        get {
            return enclosed;
        }

        set {
            enclosed = value;
            if(OnStateChange!=null)
                OnStateChange();
        }
    }
    public float Height {
        get {
            return height;
        }

        set {
            height = value;
            if(OnShapeChange!=null)
                OnShapeChange();
        }
    }
    public float Extrusion {
        get {
            return extrusion;
        }

        set {
            extrusion = value;
            if(OnShapeChange!=null)
                OnShapeChange();
            if(OnStateChange!=null)
                OnStateChange();
        }
    }
    public bool BothSideExtrusion {
        get {
            return bothSideExtrusion;
        }

        set {
            bothSideExtrusion = value;
            if(OnShapeChange != null)
                OnShapeChange();
            if(OnStateChange != null)
                OnStateChange();
        }
    }

    public IEnumerator<OutlineItem> GetEnumerator() {
        return ((IEnumerable<OutlineItem>)shapes).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<OutlineItem>)shapes).GetEnumerator();
    }

    public void AddShape( OutlineItem outlineItem ) {
        shapes.Add( outlineItem );
        outlineItem.OnUpdate += OnOutlineItemUpdate; //simply forward event
    }

    public void DropShape( OutlineItem item ) {
        shapes.Remove( item );
        item.OnUpdate -= OnOutlineItemUpdate;
    }

    public void OnOutlineItemUpdate() {
        if(OnShapeChange != null)
            OnShapeChange();
    }

    #endregion properties

    public Outline(float height) {
        this.height = height;
    }

    #region shape
    /// <summary>
    /// Returns core defining points
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector2> getRawDefiningPoints() {
        List<Vector2> points = new List<Vector2>();
        if(!complete || !enclosed)
            points.Add(shapes[0].Start);
        foreach (OutlineItem shape in shapes)
            points.AddRange(shape.getDefiningPoints());
        return points;
    }

    /// <summary>
    /// Returns extruded defining points
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector2> getExtrudedDefiningPoints() {
        Vector2[] origPoints = getRawDefiningPoints().ToArray();
        if(extrusion == 0)
            return origPoints;
        if(!enclosed) { //line mode
            if(bothSideExtrusion) {
                List<Vector2> forwardPoints = new List<Vector2>();
                List<Vector2> backwardsPoints = new List<Vector2>();
                //extrude side point
                forwardPoints.Add( extrudePointFromSegment( origPoints[0], origPoints[1], extrusion ) );
                backwardsPoints.Add( extrudePointFromSegment( origPoints[0], origPoints[1], -extrusion ) );
                for(int i = 1; i < origPoints.Count() - 1; i++) {
                    forwardPoints.Add( extrudePointFromTwoSegments( origPoints[i - 1], origPoints[i], origPoints[i + 1], extrusion ) );
                    backwardsPoints.Add( extrudePointFromTwoSegments( origPoints[i - 1], origPoints[i], origPoints[i + 1], -extrusion ) );
                }
                forwardPoints.Add( extrudePointFromSegment( origPoints[origPoints.Count() - 1], origPoints[origPoints.Count() - 2], -extrusion ) );
                backwardsPoints.Add( extrudePointFromSegment( origPoints[origPoints.Count() - 1], origPoints[origPoints.Count() - 2], +extrusion ) );
                return forwardPoints.Concat( backwardsPoints.Reverse<Vector2>() );
            } else {
                List<Vector2> backwardsPoints = new List<Vector2>();
                //extrude side point
                backwardsPoints.Add( extrudePointFromSegment( origPoints[0], origPoints[1], -extrusion ) );
                for(int i = 1; i < origPoints.Count() - 1; i++) {
                    backwardsPoints.Add( extrudePointFromTwoSegments( origPoints[i - 1], origPoints[i], origPoints[i + 1], -extrusion ) );
                }
                backwardsPoints.Add( extrudePointFromSegment( origPoints[origPoints.Count() - 1], origPoints[origPoints.Count() - 2], +extrusion ) );
                return origPoints.Concat( backwardsPoints.Reverse<Vector2>() );
            }
        } else {
            List<Vector2> extrudePoints = new List<Vector2>();
            int numPoints = origPoints.Count();
            for(int i = 1; i < numPoints; i++) {
                extrudePoints.Add( extrudePointFromTwoSegments( origPoints[(i + numPoints - 1)% numPoints], origPoints[i], origPoints[(i + 1)% numPoints], extrusion ) );
            }
            return extrudePoints;
        }

    }

    Vector2 extrudePointFromSegment( Vector2 start, Vector2 end, float distance ) {
        Vector2 diff = (end - start).normalized;
        return start + new Vector2(diff.y,-diff.x) * distance;
    }
    Vector2 extrudePointFromTwoSegments( Vector2 start, Vector2 intermediate, Vector2 end, float distance ) {
        Vector2 diff = ((end - intermediate).normalized + (intermediate - start).normalized) / 2;
        return intermediate + new Vector2( diff.y, -diff.x ) * distance;
    }

    /// <summary>
    /// Returns all defining points in clockwise direction
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector2> getRawDefiningPointsClockwise() {
        IEnumerable<Vector2> points = getRawDefiningPoints();
        //if not enclosed, then clockwise doesn't make sense
        if (!enclosed || IsClockwise(points.Select(x => new Vector3(x.x, 0, x.y))))
            return points;
        else
            return points.Reverse();
        //--------------OBSOLETE
        /*Vector3[] pointsV = points.Select( x=>new Vector3( x.x, 0, x.y ) ).ToArray();
        float totalCross = 0;
        int nP = pointsV.Length;
        for(int i = 0; i < nP; i++) {
            totalCross += Mathf.Sign( Vector3.Cross( pointsV[(i - 1 + nP) % nP] - pointsV[i], pointsV[(i + 1) % nP] - pointsV[i] ).y );
        }
        if(totalCross < 0)
            return points.Reverse();
        else
            return points;*/
    }


    /// <summary>
    /// Call when shape is changed from outside, recalculates bounds and stuff
    /// </summary>
    public void OnShapeChanged() {
        bounds = new Bounds( new Vector3(shapes[0].Start.x,height,shapes[0].Start.y), Vector3.zero );
        foreach (Vector2 vct in getRawDefiningPoints())
            bounds.Encapsulate(new Vector3(vct.x, height, vct.y) );
        bounds.Expand(0.5f);
    }

    #endregion

    public Snap CalculateSnap( Ray ray, float snapDistance ) {

        Snap closestSnap = null;

        if (bounds.IntersectRay(ray)) { //only if we intersect bounds - optimalization

            Vector3? lastPoint = null;
            IEnumerable<Vector2> points = getRawDefiningPoints();
            points = complete ? points : points.Take( points.Count() - 1 );
            foreach (Vector2 point in points ) {
                Vector3 point3d = new Vector3(point.x, height, point.y);
                //snapping to points
                float vertexToRayDistance = Vector3.Cross(point3d - ray.origin, point3d - ray.origin - ray.direction).magnitude;
                if (vertexToRayDistance < 0.5f) {
                    if ((closestSnap == null || vertexToRayDistance / 0.5f < closestSnap.snappingPriority)) {//is closer
                        closestSnap = new Snap(point3d, Vector3.up, vertexToRayDistance / 0.5f, Snap.SnappingClass.POINT);
                    }
                }
                //snapping to segments
                if (lastPoint.HasValue) {
                    Vector3 outSegment1;
                    Vector3 outSegment2;
                    //line intersection?
                    if (CalculateLineLineIntersection( lastPoint.Value, point3d, ray.origin, ray.origin + ray.direction, out outSegment1, out outSegment2 )) {
                        //segment intersection?
                        float relativePosition = Vector3.Dot( outSegment1 - lastPoint.Value, point3d - lastPoint.Value ) / (point3d - lastPoint.Value).sqrMagnitude;
                        if (relativePosition > 0 && relativePosition < 1  ) {
                            float edgeToRayDistance = (outSegment1 - outSegment2).magnitude;
                            if (edgeToRayDistance < 0.4f)
                                if ( closestSnap == null || edgeToRayDistance / 0.4f < closestSnap.snappingPriority) {
                                    closestSnap = new Snap(outSegment1, (point3d - lastPoint.Value).normalized, edgeToRayDistance / 0.4f, Snap.SnappingClass.EDGE);
                                }
                        }
                    }
                }
                lastPoint = point3d;
            }
            
        }

        return closestSnap;
    }

    /// <summary>
    /// Calculates the intersection line segment between 2 lines (not segments).
    /// Returns false if no solution can be found.
    /// </summary>
    /// <returns></returns>
    private bool CalculateLineLineIntersection(Vector3 line1Point1, Vector3 line1Point2,
        Vector3 line2Point1, Vector3 line2Point2, out Vector3 resultSegmentPoint1, out Vector3 resultSegmentPoint2) {
        // Algorithm is ported from the C algorithm of 
        // Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
        resultSegmentPoint1 = Vector3.zero;
        resultSegmentPoint2 = Vector3.zero;

        Vector3 p1 = line1Point1;
        Vector3 p2 = line1Point2;
        Vector3 p3 = line2Point1;
        Vector3 p4 = line2Point2;
        Vector3 p13 = p1 - p3;
        Vector3 p43 = p4 - p3;

        if (p43.sqrMagnitude < Mathf.Epsilon) {
            return false;
        }
        Vector3 p21 = p2 - p1;
        if (p21.sqrMagnitude < Mathf.Epsilon) {
            return false;
        }

        float d1343 = p13.x * (float)p43.x + (float)p13.y * p43.y + (float)p13.z * p43.z;
        float d4321 = p43.x * (float)p21.x + (float)p43.y * p21.y + (float)p43.z * p21.z;
        float d1321 = p13.x * (float)p21.x + (float)p13.y * p21.y + (float)p13.z * p21.z;
        float d4343 = p43.x * (float)p43.x + (float)p43.y * p43.y + (float)p43.z * p43.z;
        float d2121 = p21.x * (float)p21.x + (float)p21.y * p21.y + (float)p21.z * p21.z;

        float denom = d2121 * d4343 - d4321 * d4321;
        if (Mathf.Abs(denom) < Mathf.Epsilon) {
            return false;
        }
        float numer = d1343 * d4321 - d1321 * d4343;

        float mua = numer / denom;
        float mub = (d1343 + d4321 * (mua)) / d4343;

        resultSegmentPoint1.x = (float)(p1.x + mua * p21.x);
        resultSegmentPoint1.y = (float)(p1.y + mua * p21.y);
        resultSegmentPoint1.z = (float)(p1.z + mua * p21.z);
        resultSegmentPoint2.x = (float)(p3.x + mub * p43.x);
        resultSegmentPoint2.y = (float)(p3.y + mub * p43.y);
        resultSegmentPoint2.z = (float)(p3.z + mub * p43.z);

        return true;
    }

    #region draw calls

    public void Draw( Material outline, Material shadow, Material extrusion) {
        IEnumerable<Vector2> defPoints = getRawDefiningPoints();

        //debug
       /* foreach(Vector2 point in defPoints) {
            GraphicsHelper.DrawLine( new Vector3( point.x, height, point.y), new Vector3( point.x, height, point.y ) + Vector3.up, extrusion );
        }*/

        Vector2 lastPoint = defPoints.First();

        foreach(Vector2 point in defPoints.Skip( 1 )) {
            DrawOutlineSegment( lastPoint, point, height, outline, shadow );
            lastPoint = point;
        }

        if(enclosed)
            DrawOutlineSegment( lastPoint, defPoints.First(), height, outline, shadow );

        if(this.extrusion != 0) {
            defPoints = getExtrudedDefiningPoints();
            lastPoint = defPoints.First();
            foreach(Vector2 point in defPoints.Skip( 1 )) {
                DrawOutlineSegment( lastPoint, point, height, extrusion, shadow );
                lastPoint = point;
            }
            DrawOutlineSegment( lastPoint, defPoints.First(), height, extrusion, shadow );
        }

        

    }

    void DrawOutlineSegment( Vector2 from, Vector2 to, float height, Material mat, Material shadow ) {
        Vector3 start = new Vector3( from.x, height, from.y );
        Vector3 end = new Vector3( to.x, height, to.y );
        GraphicsHelper.DrawLine( start, end, mat ); // leveled outline (blue)
        DrawShadowedLine( start, end, height, shadow );
    }

    /// <summary>
    /// Draws a line "shadow" (line's vertical projection on terrain)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void DrawShadowedLine( Vector3 start, Vector3 end, float height, Material mat ) {
        Vector3 lastProjectedPoint = GraphicsHelper.getProjectedPoint( start );
        int numSubPoints = (int)Mathf.Floor( Vector3.Distance( start, end ) / subPointDistance ) + 1;
        for(int i = 1; i < numSubPoints; i++) {
            Vector3 interp = Vector3.Lerp( start, end, (float)i / numSubPoints );
            Vector3 projectedPoint = GraphicsHelper.getProjectedPoint( interp );
            if((lastProjectedPoint.y > height + 0.05f || lastProjectedPoint.y < height - 1) && (projectedPoint.y > height + 0.05f || projectedPoint.y < height - 1)) //only if shadow projection "makes sense"
                GraphicsHelper.DrawLine( lastProjectedPoint, projectedPoint, mat ); // outline "shadow" projection
            lastProjectedPoint = projectedPoint;
        }
    }

    #endregion

    public static bool IsClockwise(IEnumerable<Vector3> points) {
        Vector3[] pointsV = points.ToArray();
        float totalCross = 0;
        int nP = pointsV.Length;
        for (int i = 0; i < nP; i++)
            totalCross += Mathf.Sign(Vector3.Cross(pointsV[(i - 1 + nP) % nP] - pointsV[i], pointsV[(i + 1) % nP] - pointsV[i]).y);
        return totalCross > 0;
    }

}

