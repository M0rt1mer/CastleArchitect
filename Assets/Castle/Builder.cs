using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

public class Builder : MonoBehaviour {

    #region EditorSettings
    public Material outlineMat;
    public Material gizmoMat;
    public Material shadowMat;
    public Material brickMat;
    public Material snapMat;
    public Material lockMaterial;
    public Material extrusionMat;
    #endregion

    #region activeEditing
    [HideInInspector]
    public Outline activeOutline = null;
    [HideInInspector]
    public OutlineItem activeOutlineItem = null;
    [HideInInspector]
    private BuildingData selectedBuildingData;
    public BuildingData SelectedBuildingData {
        get {
            return selectedBuildingData;
        }

        set {
            selectedBuildingData = value;
            if(selectedBuildingData != null) {
                GameObject gobj;
                if(activeBuilding != null) {
                    gobj = activeBuilding.gameObject;
                    gobj.transform.SetParent( transform );
                    Destroy( activeBuilding );
                } else {
                    gobj = new GameObject();
                    gobj.transform.SetParent( transform, false );
                }

                activeBuilding = PlacedBuilding.CreatePlacedBuilding( selectedBuildingData, activeOutline, gobj );
            } else {
                if(activeBuilding != null)
                    Destroy( activeBuilding.gameObject );
                activeBuilding = null;
            }
        }
    }
    private PlacedBuilding activeBuilding = null;

    private Snap lockedSnap = null;
    private Vector2 lockedDirection;
    private float originalHeight; // for outline height editing
    #endregion

    #region allEditing
    private List<Outline> builtObjects = new List<Outline>();
    //private Dictionary<Outline, Mesh> meshes = new Dictionary<Outline, Mesh>();
    #endregion

    public float subPointDistance = 0.1f;
    private EventSystem eventSystem;

    private static float doubleClickTime = 0.1f;
    private float timeLastClick = 0;
    public bool didDoubleClick = true;

    

    // Use this for initialization
    void Start () {
        GraphicsHelper.Initialize();
        //-----------------------------
        GameObject.FindObjectOfType<Tooltip>().tooltipCallback.Add( getTooltip );
        eventSystem = GameObject.FindObjectOfType<EventSystem>();
    }

    // Update is called once per frame
    void Update() {

        //calculate basic "data"
        RaycastHit hit = new RaycastHit();
        Snap snap = CalculateSnap(Camera.main.ScreenPointToRay(Input.mousePosition));
        bool didHit = !eventSystem.IsPointerOverGameObject() && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100);

        if(Input.GetButtonUp( "Target" )) {
            timeLastClick = Time.time;
        } else if(Input.GetButtonDown( "Target" )) {
            didDoubleClick = Time.time - timeLastClick < doubleClickTime;
        }

        UpdateLockDirection(snap);
        if (activeOutlineItem != null)
            UpdateModeOutlineItem(didHit, hit, snap);
        else if (activeOutline != null)
            UpdateModeOutline(didHit, hit, snap);
        else
            UpdateModeBasic(didHit, hit, snap);

        /*foreach(Outline outline in builtObjects) {
            DrawOutline( outline );
        }*/

    }


    #region updateModes

    void UpdateModeOutline(bool didHit, RaycastHit hit, Snap snap) {

        if (Input.GetButtonUp("Build") && activeBuilding != null) {
            builtObjects.Add(activeOutline);
            activeBuilding.Build();
            activeOutline = null;
            foreach(PlacedBuilding plbd in GetComponentsInChildren<PlacedBuilding>())
                if(plbd != activeBuilding)
                    plbd.UpdateWorld();
            activeBuilding = null;
        }

        if (Input.GetButton("AdjustHeight")) {
            if (snap == null) {
                //TODO: changed from center of bounds, check if works
                Vector3 closest = activeOutline.getRawDefiningPoints().First();
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                //find closest point
                foreach (Vector3 point in activeOutline.getRawDefiningPoints())
                    if (Vector3.Dot(cameraRay.direction, point - cameraRay.origin) < Vector3.Dot(cameraRay.direction, closest - cameraRay.origin))
                        closest = point;
                //find "desired" height
                float distance = Vector3.Dot(cameraRay.direction, cameraRay.origin - closest);
                float height = (cameraRay.origin + cameraRay.direction * distance).y/4;
                if(Input.GetButtonDown( "AdjustHeight" )) {
                    originalHeight = activeOutline.Height - height; // offset the height to that the point of clicking = zero height modification
                }
                activeOutline.Height = height + originalHeight;
                activeOutline.OnShapeChanged();
            }
            else
                activeOutline.Height = snap.snappingPoint.y;
        }

        if(Input.GetKeyDown( KeyCode.E )) {
            if(activeOutline.Extrusion == 0)
                activeOutline.Extrusion = 1;
            else if(activeOutline.BothSideExtrusion == true) {
                activeOutline.BothSideExtrusion = false;
                activeOutline.Extrusion = 0;
            } else
                activeOutline.BothSideExtrusion = true;
        }

        DrawModeOutline(didHit, hit, snap);
    }

    void UpdateModeOutlineItem(bool didHit, RaycastHit hit, Snap snap) {

        Vector3? targetPoint = snap!=null? snap.snappingPoint : ( didHit ? hit.point : (Vector3?)null );

        if (targetPoint.HasValue) {
            Vector2 targetPoint2d = new Vector2(targetPoint.Value.x, targetPoint.Value.z);
            if ( lockedSnap != null) {
                targetPoint2d = Vector2.Dot(targetPoint2d - activeOutlineItem.Start, lockedDirection) * lockedDirection + activeOutlineItem.Start;
            }                
            activeOutlineItem.End = targetPoint2d;
            if (Input.GetButtonUp("Target") && !Input.GetButton("LockDirection") ) { //create new outline item -- on click
                activeOutlineItem = new OutlineItem(targetPoint2d, targetPoint2d);
                activeOutline.AddShape(activeOutlineItem);
                activeOutline.OnShapeChanged();
            }
        }
        else
            activeOutlineItem.End = activeOutlineItem.Start;

        if(Input.GetKeyUp( KeyCode.C )) { //close
            activeOutline.Last().End = activeOutline.First().Start;
            activeOutlineItem = null; //changes mode to OutlineMode
            activeOutline.Complete = activeOutline.Enclosed = true;
        } else if ( Input.GetKeyUp(KeyCode.F) ) {
            activeOutline.DropShape( activeOutline.Last() ); //remove last segment (still open segment)
            activeOutlineItem = null;
            activeOutline.Complete = true;
        }
        else if(Input.GetKeyDown( KeyCode.E )) {
            if(activeOutline.Extrusion == 0)
                activeOutline.Extrusion = 1;
            else if(activeOutline.BothSideExtrusion == true) {
                activeOutline.BothSideExtrusion = false;
                activeOutline.Extrusion = 0;
            } else
                activeOutline.BothSideExtrusion = true;
        }

        activeOutline.OnShapeChanged();

        DrawModeOutlineItem(didHit, hit, snap);
    }

    void UpdateModeBasic(bool didHit, RaycastHit hit, Snap snap) {

        Vector3? targetPoint = snap != null ? snap.snappingPoint : (didHit ? hit.point : (Vector3?)null);

        if ( targetPoint.HasValue && Input.GetButtonUp("Target") && Input.GetButton("Build")) { //create new shape
            activeOutline = new Outline(targetPoint.Value.y);
            activeOutlineItem = new OutlineItem(new Vector2(targetPoint.Value.x, targetPoint.Value.z), new Vector2(targetPoint.Value.x, targetPoint.Value.z));
            activeOutline.AddShape(activeOutlineItem);
            activeOutline.OnShapeChanged();
        }

        DrawModeBasic(didHit, hit, snap);
    }

    void UpdateLockDirection(Snap snap) {
        if (Input.GetButton("LockDirection") && Input.GetButtonUp("Target")) {
            if (snap != null && snap.cls == Snap.SnappingClass.EDGE) {
                lockedSnap = snap;
                lockedDirection = new Vector2(snap.snappingDirection.x, snap.snappingDirection.z); //initially the snapping direction
            }
            else
                lockedSnap = null;

        }
        else if( lockedSnap!= null && Input.GetButton("LockDirection") && Input.GetButton("Move Camera")) { //new lock angle selection mode
            Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distanceOnRay = (lockedSnap.snappingPoint.y - cursorRay.origin.y) / cursorRay.direction.y;
            Vector3 intersection = cursorRay.origin + cursorRay.direction * distanceOnRay;
//            Debug.Log(intersection);
            Vector2 relativePoint = new Vector2(intersection.x - lockedSnap.snappingPoint.x, intersection.z - lockedSnap.snappingPoint.z);
            //convert to angle
            float baseAngle = Mathf.Atan2(lockedSnap.snappingDirection.z, lockedSnap.snappingDirection.x);
            float angle = (Mathf.Atan2(relativePoint.y, relativePoint.x) - baseAngle) * Mathf.Rad2Deg;
            //float oldAngle = angle;
            //round
            angle = Mathf.Round(angle / 22.5f) * 22.5f * Mathf.Deg2Rad ;
            //Debug.Log(oldAngle +" to " +angle * Mathf.Rad2Deg +" base" + baseAngle);
            //convert back
            lockedDirection = new Vector2(Mathf.Cos(angle + baseAngle), Mathf.Sin(angle + baseAngle));
        }
        DrawLockDirection(snap);
    }

    #endregion

    #region drawModes
    void DrawModeOutline(bool didHit, RaycastHit hit, Snap snap) {
        if(activeOutline != null) //can be null, if player built the building this update
            //DrawOutline(activeOutline);
            activeOutline.Draw( outlineMat, shadowMat, extrusionMat );
        if ( snap != null && Input.GetButton("AdjustHeight"))
            DrawSnap(snap);
    }

    void DrawModeOutlineItem(bool didHit, RaycastHit hit, Snap snap) {
        if (snap != null)
            DrawSnap(snap);
        else if (didHit)
            DrawHit(hit);
        activeOutline.Draw( outlineMat, shadowMat, extrusionMat );
    }

    void DrawModeBasic(bool didHit, RaycastHit hit, Snap snap) {
        if (Input.GetButton("Build") && snap != null) //display snapping If player is holding AlternateMode
            DrawSnap(snap);
    }

    void DrawLockDirection(Snap snap) {
        if (lockedSnap != null) {

            if (Input.GetButton("LockDirection") && Input.GetButton("Move Camera")) { //new lock angle selection mode
                DrawLine(lockedSnap.snappingPoint, lockedSnap.snappingPoint + new Vector3(lockedDirection.x,0,lockedDirection.y), lockMaterial);
                Vector3 perpendicularSnapping = new Vector3(lockedSnap.snappingDirection.z, 0, -lockedSnap.snappingDirection.x);
                DrawLine(lockedSnap.snappingPoint - lockedSnap.snappingDirection, lockedSnap.snappingPoint + lockedSnap.snappingDirection, shadowMat);
                DrawLine(lockedSnap.snappingPoint - perpendicularSnapping, lockedSnap.snappingPoint + perpendicularSnapping, shadowMat);
                DrawLine(lockedSnap.snappingPoint - lockedSnap.snappingDirection * 0.5f - perpendicularSnapping * 0.5f, lockedSnap.snappingPoint + lockedSnap.snappingDirection * 0.5f + perpendicularSnapping * 0.5f, shadowMat);
                DrawLine(lockedSnap.snappingPoint - lockedSnap.snappingDirection * 0.5f + perpendicularSnapping * 0.5f, lockedSnap.snappingPoint + lockedSnap.snappingDirection * 0.5f - perpendicularSnapping * 0.5f, shadowMat);
            }
            //just display snapping point
            else {
                DrawLine(lockedSnap.snappingPoint, lockedSnap.snappingPoint + new Vector3(lockedDirection.x, 0, lockedDirection.y), lockMaterial);
            }
        }
        
    }
    #endregion



    public Snap CalculateSnap( Ray ray ) {
        Snap closestSnap = null;
        //go through all
        IEnumerable<Outline> allOutlines = activeOutline == null ? builtObjects : builtObjects.Concat(new Outline[] { activeOutline }); //snap to active outline, if exists
        foreach (Outline outline in allOutlines ) {
            Snap snap = outline.CalculateSnap(ray, 0.5f);
            if ((snap != null) && ( closestSnap == null || Vector3.SqrMagnitude(snap.snappingPoint-Camera.main.transform.position) < Vector3.SqrMagnitude(closestSnap.snappingPoint-Camera.main.transform.position)))
                closestSnap = snap;
        }
        //calculate current outline snapping
        //TODO


        return closestSnap;
    }

    public IEnumerable<string> getTooltip() {
        List<string> lst = new List<string>();
        if (Input.GetAxis("Help") > 0) {
            if (activeOutline == null)
                lst.Add("[Target] + [Build] create outline");
            if (activeOutlineItem != null)
                lst.Add("[Target] new point\n");
            if (activeOutlineItem != null && activeOutline != null && activeOutline.Count() > 2)
                lst.Add("[Build] close outline\n");
            if (activeOutline != null && activeOutlineItem == null && activeBuilding != null)
                lst.Add("[Build] build\n");
        }
        return lst;
    }

    void DrawHit(RaycastHit  hit) {
        
        if (activeOutlineItem != null) {
            DrawLine(new Vector3(hit.point.x, activeOutline.Height, hit.point.z), hit.point, gizmoMat );
        }


    }

    #region draw calls

    /*void DrawOutline( Outline outline ) {
        IEnumerable<Vector2> defPoints = outline.getRawDefiningPoints();
        Vector2 lastPoint = defPoints.First();

        foreach(Vector2 point in defPoints.Skip( 1 )) {
            DrawOutlineSegment( lastPoint, point, outline.height );
                lastPoint = point;
        }

        if(outline.enclosed)
            DrawOutlineSegment( lastPoint, defPoints.First(), outline.height );
    }*/

    void DrawOutlineSegment( Vector2 from, Vector2 to, float height ) {
        Vector3 start = new Vector3( from.x, height, from.y );
        Vector3 end = new Vector3( to.x, height, to.y );
        DrawLine( start, end, outlineMat ); // leveled outline (blue)
        DrawShadowedLine( start, end, height );
    }

    /// <summary>
    /// Draws a line "shadow" (line's vertical projection on terrain)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void DrawShadowedLine( Vector3 start, Vector3 end, float height ) {
        Vector3 lastProjectedPoint = getProjectedPoint(start);
        int numSubPoints = (int)Mathf.Floor( Vector3.Distance( start, end ) / subPointDistance ) + 1;
        for(int i = 1; i < numSubPoints; i++) {
            Vector3 interp = Vector3.Lerp( start, end, (float)i / numSubPoints );
            Vector3 projectedPoint = getProjectedPoint( interp );
            if((lastProjectedPoint.y > height + 0.05f || lastProjectedPoint.y < height - 1) && (projectedPoint.y > height + 0.05f || projectedPoint.y < height - 1)) //only if shadow projection "makes sense"
                DrawLine( lastProjectedPoint, projectedPoint, shadowMat ); // outline "shadow" projection
            lastProjectedPoint = projectedPoint;
        }
    }

    void DrawSnap(Snap snap) {

        if (snap.cls == Snap.SnappingClass.POINT) {
            DrawSphere( snap.snappingPoint, snapMat );
        }
        else {
            Vector3 point = snap.snappingPoint;
            float sizeMultiplier = Vector3.Distance(point, Camera.main.transform.position) / 10;
            Quaternion rot = Quaternion.LookRotation(snap.snappingDirection, Vector3.up);

            DrawLine(point - rot * Vector3.up * sizeMultiplier, point + rot * Vector3.up * sizeMultiplier, snapMat);
            DrawLine(point - rot * Vector3.forward * sizeMultiplier, point + rot * Vector3.forward * sizeMultiplier, snapMat);
            DrawLine(point - rot * Vector3.left * sizeMultiplier, point + rot * Vector3.left * sizeMultiplier, snapMat);
        }


    }

    #endregion

    Vector3 getProjectedPoint(Vector3 orig) {
        RaycastHit hit;
        if (Physics.Raycast(orig + Vector3.up * 200, -Vector3.up, out hit, 1000)) {
            orig.y = hit.point.y;
        }
        return orig;
    }

    void DrawLine(/*LinesGR lines,*/ Vector3 start, Vector3 end, Material mat) {
        //lines.AddLine(lines.MakeQuad( new Vector3(start.x,start.y,height), new Vector3(end.x, end.y, height), 0.003f), false);
        if (end != start) {
            Quaternion facing = Quaternion.LookRotation(end - start) ;
            Graphics.DrawMesh( GraphicsHelper.cubeModel, Matrix4x4.TRS((start + end) / 2, facing, new Vector3(0.05f, 0.05f, Vector3.Distance(start, end) )), mat, 0, null, 0, null, false);
        }
    }

    void DrawSphere(Vector3 start, Material mat) {
        Graphics.DrawMesh( GraphicsHelper.sphereModel, start, Quaternion.identity, mat, 0);
    }


    private static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {

        Vector2 a = p2 - p1;
        Vector2 b = p3 - p4;
        Vector2 c = p1 - p3;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float alphaDenominator = a.y * b.x - a.x * b.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float betaDenominator = a.y * b.x - a.x * b.y;

        bool doIntersect = true;

        if (alphaDenominator == 0 || betaDenominator == 0) {
            doIntersect = false;
        }
        else {

            if (alphaDenominator > 0) {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator) {
                    doIntersect = false;

                }
            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator) {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0) {
                if (betaNumerator < 0 || betaNumerator > betaDenominator) {
                    doIntersect = false;
                }
            } else if (betaNumerator > 0 || betaNumerator < betaDenominator) {
                doIntersect = false;
            }
        }

        return doIntersect;
    }


}
