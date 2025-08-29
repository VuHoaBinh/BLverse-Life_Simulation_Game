using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Astar : MonoBehaviour
{
    // Start is called before the first frame update
    public Map map;
    public Node startNode;
    public Node goalNode;
    public GameObject dot; // Prefab for visualizing the path
    public List<Vector3> path;
    // Update is called once per frame
    public void FindPath(Node start, Node goal)
    {
        path = new List<Vector3>(); //Khởi tạo danh sách đường đi
        Node curentNode = start;

        /*Khai báo 2 hashset là openset và closedset 
        - Không dubpltcate
        - mỗi phần tử đưa vào sẽ có 1 hashcode khác nhau và dựa vào đó để tìm index chứa
        phần từ
        */
        HashSet<Node> openSet = new HashSet<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(curentNode);
        while (openSet.Count > 0) //chạy khi openset còn phần tử
        {
            curentNode = GetLowestCostNode(openSet); // Lấy giá trí Node có chi phí nhỏ nhất
            if (Vector3.Distance(curentNode.position, goal.position) < 0.001f) // Điều kiện dừng
            {
                // Debug vẽ đường đi
                map.mapUI.deleteCircle("path"); // Xóa các vòng tròn cũ
                Node node = curentNode;
                while (node != null)
                {
                    path.Add(map.changeCellPos(node.position));
                    Instantiate(dot, map.changeCellPos(node.position), Quaternion.identity);
                    node = node.parent;
                }
                path.RemoveAt(path.Count - 1);
                path.Reverse(); // Đảo ngược đường đi để từ start đến goal
                // Debug.Log("Path found with " + path.Count + " nodes.");
                return;
            }

            openSet.Remove(curentNode); // Xóa Node hiện tại khỏi tập hợp mở
            closedSet.Add(curentNode); // Thêm Node hiện tại vào tập hợp đã đóng

            addNeighborsByDirection(curentNode, goal); // Thêm các Node lân cận vào Node hiện tại

            // Duyệt qua các Node lân cận
            foreach (var neighbor in curentNode.neighbors)
            {
                if (!map.checkTileAtPosition(new Vector3Int((int)neighbor.Key.position.x, (int)neighbor.Key.position.y, 0)))
                {
                    if (closedSet.Contains(neighbor.Key)) //Nếu Node lân cận đã có trong tập đóng thì bỏ qua
                    {
                        continue; // Ignore already evaluated nodes
                    }

                    if (!openSet.Contains(neighbor.Key)) //Nếu chưa có trong tập mở thì thêm vào
                    {
                        neighbor.Key.parent = curentNode;
                        openSet.Add(neighbor.Key);
                    }
                }
            }
        }

        // In ra đường đi cuối cuùng
        Debug.Log("No path found to goal node.");
    }
    public Node GetLowestCostNode(HashSet<Node> openSet)
    {
        Node lowestCostNode = null;
        float lowestCost = float.MaxValue;

        foreach (var node in openSet)
        {
            if (node.totalCost < lowestCost)
            {
                lowestCost = node.totalCost;
                lowestCostNode = node;
            }
        }

        return lowestCostNode;
    }
    public void calcHeuristic(Node node, Node goal)
    {
        // Calculate heuristic value for the node
        // This is just a placeholder for the method
        node.heuristic = Vector3.Distance(node.position, goal.position);
    }

    public bool checkIsInMap(Vector3 position)
    {
        // Kiểm tra postion có trong biên hay không
        int minX = (int)map.verticesList[0].transform.position.x;
        int maxX = (int)map.verticesList[1].transform.position.x;
        int minY = (int)map.verticesList[0].transform.position.y;
        int maxY = (int)map.verticesList[2].transform.position.y;

        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            return false;
        }
        return true;
    }
    public void addNeighborsByDirection(Node node, Node goal)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 direction = Vector3.zero;
            switch (i)
            {
                case 0: // Up
                    direction = Vector3.up;
                    break;
                case 1: // Down
                    direction = Vector3.down;
                    break;
                case 2: // Left
                    direction = Vector3.left;
                    break;
                case 3: // Right
                    direction = Vector3.right;
                    break;
                case 4: // Left top
                    direction = Vector3.left + Vector3.up;
                    break;
                case 5: // Left bottom
                    direction = Vector3.left + Vector3.down;
                    break;
                case 6: // Right top
                    direction = Vector3.right + Vector3.up;
                    break;
                case 7: // Right bottom
                    direction = Vector3.right + Vector3.down;
                    break;
            }
            Vector3 neighborPosition = node.position + direction;
            /*Cần tối ưu là nếu node hàng xóm đó đã xét rồi thì không nên xét nữa*/
            if (checkIsInMap(neighborPosition))
            {
                Node neighborNode = new Node(neighborPosition);
                calcHeuristic(neighborNode, goal);
                float distance = Vector3.Distance(neighborNode.position, node.position);
                neighborNode.distance = node.distance + distance; // Cập nhật khoảng cách từ Node hiện tại đến Node lân cận
                neighborNode.totalCost = neighborNode.distance + neighborNode.heuristic;
                node.AddNeighbor(neighborNode, neighborNode.heuristic + distance);
            }
        }
    }
}
