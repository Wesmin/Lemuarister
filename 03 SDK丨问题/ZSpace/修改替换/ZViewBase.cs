using UnityEngine;
using zSpace.zView;

public class ZViewBase : MonoBehaviour
{
    private ZView _zView = null;
    void Start()
    {
        _zView = GameObject.FindObjectOfType<ZView>();
        if (_zView == null)
        {
            Debug.LogError("Unable to find reference to zSpace.zView.ZView Monobehaviour.");
            this.enabled = false;
            return;
        }
    }
}
