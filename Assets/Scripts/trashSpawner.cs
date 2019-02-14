﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trashSpawner : MonoBehaviour {
    public GameObject[] trashPrefab;

    public static int MAX_TRASH_COUNT = 2;
    private int trashCount = 0;

    public float energyUntilHit = 2;
    public int hitsUntilDrop = 2;
    private GameObject dropPoint;
    private float dropDelay = 0, spawnDelay = 0;
    private int hit = 0;

    private float shake = 0;

    private GameObject item;

    void Start() {
        dropPoint = transform.Find("Drop").gameObject;
    }

    void FixedUpdate() {
        if(spawnDelay > 0) spawnDelay -= Time.deltaTime;
        if(item == null && spawnDelay <= 0) item = SpawnItem();
        if(shake > 0) {
            shake -= Time.deltaTime;
            transform.rotation = Quaternion.Euler(Mathf.Sin(shake*shake), Mathf.Cos(shake*shake*5)*2, Mathf.Sin(shake*shake));
        }
        else transform.rotation = Quaternion.Euler(Mathf.LerpAngle(transform.rotation.x, 0, Time.deltaTime*2), Mathf.LerpAngle(transform.rotation.y, 0, Time.deltaTime*2), Mathf.LerpAngle(transform.rotation.z, 0, Time.deltaTime*2));
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "Hitter" && item != null) {
            if(other.GetComponentInParent<player>().energy > energyUntilHit && Mathf.Abs(other.transform.localEulerAngles.y) < 45) {
                hit++;
                audioManager.PLAY_SOUND("Hit", transform.position, 1200, Random.Range(0.9f, 1.2f));
                Shake();
                if(hit >= hitsUntilDrop) DropItem();
            }
        }
    }

    private void Shake() {
        shake = 3;
    }

    private GameObject SpawnItem() {
        GameObject go = Instantiate(trashPrefab[Random.Range(0, trashPrefab.Length)]);
        go.transform.rotation = Quaternion.identity;
        go.GetComponent<Rigidbody>().isKinematic = true;
        go.transform.position = dropPoint.transform.position;
        go.transform.SetParent(transform);
        audioManager.PLAY_SOUND("Plop", transform.position, 1200, Random.Range(0.9f, 1.2f));
        return go;
    }

    private void DropItem() {
        if(item == null) return;
        item.transform.SetParent(null);
        item.GetComponent<Rigidbody>().isKinematic = false;
        item.GetComponent<Rigidbody>().AddForce(0, 0, -200);
        item = null;
        spawnDelay = 10;
        hit = 0;
    }
}
