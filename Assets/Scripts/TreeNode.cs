using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{

    Vector3 position;
    public Vector3 Position { get => position; set => position = value; }
    List<TreeBud> buds;
    public List<TreeBud> Buds { get => buds; }
    int depth;
    public int Depth { get => depth; set => depth = value; }

    public TreeNode(Vector3 position, int depth) 
    {
        this.position = position;
        this.buds     = new List<TreeBud>();
        this.depth    = depth;  
    } 

    public TreeBud CreateSideBuds(TreeBud bud, TreeInfo info)
    {
        if (bud.Order + 1 > info.MaxDepth) return null;

        Vector3 newT = (
                (Quaternion.AngleAxis(Random.Range(-45.0f, 45.0f), bud.Binormal)) * 
                (Quaternion.AngleAxis(Random.Range(-45.0f, 45.0f), bud.Normal) * bud.Tangent)
            ).normalized;
        Vector3 newN = (bud.Normal * Mathf.Cos(30.0f * Mathf.Deg2Rad) + bud.Binormal * Mathf.Sin(30.0f  * Mathf.Deg2Rad)).normalized;
        Vector3 newB = Vector3.Cross(newT, newN);

        TreeBud newBud = new TreeBud(newT, newN, newB, bud.Order + 1);

        return newBud;
    }

    public TreeInternode CreateInternode(TreeBud bud, TreeInfo info)
    {
        float dT = (info.StepSize) * ((info.MaxDepth + 0.5f - bud.Order) / info.MaxDepth);

        Vector3 newPos = this.Position + bud.Tangent * dT;
        TreeNode newNode = new TreeNode(newPos, this.depth + 1);

        Vector3 newT;
        if (info.BranchesUp)
        {
            newT = TreeMeshUtils.MixTangent(bud.Tangent, Vector3.up);
        }
        else
        {
            Vector3 H = (bud.Tangent.x == 0.0f && bud.Tangent.z == 0.0f) ?
                            new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) : 
                            new Vector3(bud.Tangent.x, 0.0f, bud.Tangent.z); 
            newT = TreeMeshUtils.MixTangent(bud.Tangent, H);
        }

        Vector3 newB = Vector3.Cross(newT, bud.Normal).normalized;
        Vector3 newN = Vector3.Cross(newB, newT).normalized;
        
        TreeBud newBud = new TreeBud(newT, newN, newB, bud.Order);
        
        newNode.Buds.Add(newBud);
        this.Buds.Remove(bud);
        
        float thickness = (1.0f) * ((info.MaxDepth + 0.01f - bud.Order) / info.MaxDepth);
        TreeInternode internode = new TreeInternode(this, newNode, thickness);

        return internode;
    }

    public void GrowLeaf(Vector3 pos, int order, TreeInfo info, GameObject parent)
    {
        float rad = (Mathf.Exp(-0.012f * this.Depth)) * (2.5f * info.StepSize) * ((info.MaxDepth + 0.01f - order) / info.MaxDepth);
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        temp.transform.position = pos;
        temp.transform.localScale = Vector3.one * rad;
        temp.transform.parent = parent.transform;
        temp.GetComponent<Renderer>().material.color = info.LeafColor;
    }

}
