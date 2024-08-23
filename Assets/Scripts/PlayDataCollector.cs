using UnityEngine;
    

public class PlayDataCollector :MonoBehaviour
{
    public static bool IsOnCheckPoint = false;
    public static bool IsOnEndPoint = false;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            IsOnCheckPoint = false;
        }
        else if (other.gameObject.CompareTag("CheckPoint"))
        {
            IsOnCheckPoint = true;
        }
        else if (other.gameObject.CompareTag("StartPoint"))
        {
            IsOnCheckPoint = false;
        }
        else if (other.gameObject.CompareTag("EndPoint"))
        {
            IsOnCheckPoint = true;
            IsOnEndPoint = true;
        }
    }
}
