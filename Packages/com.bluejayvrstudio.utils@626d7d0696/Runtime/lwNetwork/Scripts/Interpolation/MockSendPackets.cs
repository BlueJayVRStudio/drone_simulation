using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MockSendPackets : MonoBehaviour
{
    [SerializeField] Interpolations interpolations;

    [SerializeField] float tickrate = 60.0f;
    [SerializeField] int delay = 90;
    float interval;
    float timer = 0f;

    void Start() {
        interval = 1 / tickrate;
    }

    void Update() {
        if (interpolations == null) return;
    
        timer += Time.deltaTime;
        if (timer >= interval) {
            timer = 0f;
            delayed_send(transform.position, transform.rotation);
        }
    }

    async void delayed_send(Vector3 position, Quaternion rotation) {
        await Task.Delay(delay);
        if (Random.Range(0, 10) < 0) return;
        interpolations.set_transform(position, rotation);
    }
}
