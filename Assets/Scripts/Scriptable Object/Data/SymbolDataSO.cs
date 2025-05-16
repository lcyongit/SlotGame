using UnityEngine;

[CreateAssetMenu(fileName = "SymbolDataSO", menuName = "Scriptable Objects/Data/SymbolDataSO")]
public class SymbolDataSO : ScriptableObject
{
    public Symbol symbol;      // �ϮצW��
    public Sprite icon;            // �����ϥܡ]�i�b Inspector �]�w�^
    public float probability;      // �X�{���v�]�`�M�i���N�A�|�����зǤơ^
    public int payoutMultiplier;   // �s�u�o�����v
    public bool isWild;            // �O�_�O Wild �ʷf
    public bool isScatter;         // �O�_�O Scatter �����Ϯ�

}
