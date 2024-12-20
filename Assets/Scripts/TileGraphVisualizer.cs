using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGraphVisualizer : MonoBehaviour
{
    public Tilemap tilemap;
    public TextMesh textMesh; // El Tilemap de Unity

    public bool draw = true;
    public Dictionary<Vector2Int, TileNode> nodes = new Dictionary<Vector2Int, TileNode>();

    // Color para dibujar los nodos y las conexiones
    public Color nodeColor = Color.red;
    public Color connectionColor = Color.green;

    void Start()
    {
        // Genera el grafo del Tilemap
        GenerateGraph();
    }

    // Generar el grafo de nodos
    void GenerateGraph()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPos = new Vector3Int(pos.x, pos.y, pos.z);

            if (tilemap.HasTile(localPos))
            {
                Vector2Int gridPos = new Vector2Int(localPos.x, localPos.y);

                // Crea un nodo en esa posición si no existe ya
                if (!nodes.ContainsKey(gridPos))
                {
                    TileNode node = new TileNode(gridPos);
                    nodes.Add(gridPos, node);
                }
            }
        }

        // Conectar con los vecinos
        ConnectNeighbors();
    }

    void ConnectNeighbors()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };
        foreach (KeyValuePair<Vector2Int, TileNode> kvp in nodes)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = kvp.Key + direction;

                if (nodes.ContainsKey(neighborPos))
                {
                    nodes[kvp.Key].AddNeighbor(nodes[neighborPos]);
                }
            }
        }
    }

    // Devuelve el nodo asociado a una posición del Tilemap
    public TileNode GetNodeAtPosition(Vector2Int position)
    {
        return nodes.ContainsKey(position) ? nodes[position] : null;
    }

    // Dibuja los Gizmos para visualizar el grafo en el Editor

    void CreateTextAtNode(TileNode node)
    {
        // Crear un nuevo GameObject para contener el TextMesh
        GameObject textObject = new GameObject("NodeCoordinates");

        // Añadir un componente TextMesh para mostrar texto en el mundo
        TextMesh textMesh = textObject.AddComponent<TextMesh>();

        // Asignar las coordenadas del nodo como texto
        textMesh.text = $"({node.position.x}, {node.position.y})";

        // Posicionar el texto en el mundo, un poco por encima del nodo
        textObject.transform.position = new Vector3(node.position.x, node.position.y + 0.5f, 0f);

        // Ajustar el tamaño y la apariencia del texto
        textMesh.characterSize = 0.2f;
        textMesh.color = Color.white; // Puedes cambiar el color del texto aquí
    }

    public void GenerateTacticalPoint(
        TileNode node,
        AgentType agentType,
        int cost,
        GameObject visualMarkerPrefab = null
    )
    {
        // Configurar el costo táctico para el agente especificado
        node.SetTacticalCost(agentType, cost);

        // Crear un marcador visual en la posición del nodo, si se proporciona
        if (visualMarkerPrefab != null)
        {
            GameObject marker = Instantiate(
                visualMarkerPrefab,
                node.gameObject.transform.position,
                Quaternion.identity
            );

            // Vincular el marcador al nodo (opcional)
            marker.transform.parent = node.gameObject.transform;
        }
    }

    void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0 || !draw)
            return;

        foreach (var node in nodes.Values)
        {
            node.DrawGizmo();
        }
    }
}
