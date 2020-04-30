﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    private Touch touch;
    private int numTouch;
    private Vector3 endPos;
    private bool loadShoot;
    private bool okShoot;
    private Player player;
    private bool restart;
    private float minShoot;
    private float maxShoot;
    private int layerMaskPlayer;  
    private LayerMask layerMaskFight = 1 << 12;
    private bool shootFlag;

    // Start is called before the first frame update

    // Update is called once per frame

    private void Awake()
    {
        loadShoot = false;
        okShoot = true;       
        minShoot= GameObject.Find("ShootLimitDown").transform.position.y; //M: limite inferiore e superiore che contraddistingue
        maxShoot= GameObject.Find("ShootLimitUp").transform.position.y;//un passaggio da un tiro
        layerMaskPlayer= 1 << LayerMask.NameToLayer("Player");
        shootFlag = false;
    }
    private void Start()
    {
        
         }
    void Update()
    {
        numTouch = Input.touchCount;

        if (numTouch > 0)
        {
            for (int i = 0; i < numTouch; i++)
            {
                touch = Input.GetTouch(i);
                
                ///////////// FASE BEGAN
                if (touch.phase == TouchPhase.Began)
                {
                    RaycastHit2D hitPlayer = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), (Input.GetTouch(i).position), layerMaskPlayer);
                    RaycastHit2D hitFight = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), (Input.GetTouch(i).position), layerMaskFight);


                    if (hitPlayer.collider)
                    {

                        GameObject obj = hitPlayer.collider.gameObject;

                        if (obj.tag == "Player")
                        {

                            player = GameObject.Find(obj.name).GetComponent<Player>();
                            if (player.keep || player.keepBoa)
                            {
                                loadShoot = true;
                                player.CreateSignalShoot();
                                player.signalOK = okShoot;
                            }

                        }
                        if (obj.tag == "Battle")
                        {
                            obj.GetComponent<Battle>().numclick++;
                        }
                    }
                    if (hitFight.collider != null)
                    {
                        FightManager obj = GameObject.Find(hitFight.collider.name).GetComponent<FightManager>();
                        if (obj != null)
                        {
                            obj.P1AddClickLocal();
                            Debug.Log(obj.numClickP1);
                        }

                    }
                }

                ////// FASE MOVED
                if (touch.phase == TouchPhase.Moved && loadShoot && player != null)
                {
                   // Debug.Log(Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), player.transform.position) + " : distanza del passaggio o tiro");
                    if (player.keep && !player.keepBoa)
                    {

                        Utility.RotateObjToPoint(player.gameObject, Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position));
                    }

                    if (Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position).x <= GameObject.Find("LimitLeft").transform.position.x || Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position).x >= GameObject.Find("LimitRight").transform.position.x)
                    {
                        
                        okShoot = false;
                    }
                    else
                    {   if (Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), player.transform.position) < 9)
                        {
                            okShoot = false;
                        }
                        else
                            okShoot = true;
                    }
                    player.signalOK = okShoot;

                }
                //FASE ENDED
                if (touch.phase == TouchPhase.Ended && loadShoot && player != null)
                {


                    Vector3 destination = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);


                    if (minShoot < destination.y && destination.y < maxShoot)//decidiamo se è un passaggio o un tiro
                    {
                        
                        shootFlag = false;
                    }
                    else
                    {
                       
                        shootFlag = true;

                    }

                    if (okShoot && !player.keepBoa)
                    {
                        player.LoadShoot(destination, shootFlag, 0);  //se non è una boa in possesso di palla

                    }
                    else                //IL GIOCATORE è UNA BOA BISOGNA DECIDERE IL TIPO DI TIRO A SENCONDA DI DOVE INDIRIZZIAMO IL CURSORE
                    if (okShoot && player.keepBoa)
                    {
                        LayerMask mask = 1 << 4; //strato player
                        RaycastHit2D hitBoa = Physics2D.Raycast(player.transform.parent.transform.position, (Ball.current.transform.position - player.transform.parent.transform.position).normalized, 20, mask);
                        Debug.DrawRay(player.transform.parent.transform.position, (Ball.current.transform.position - player.transform.parent.transform.position).normalized * 15, Color.black, 3);

                        if (hitBoa.collider != null)
                        {

                          //  Debug.Log(player.name + " ->" + hitBoa.collider.name);

                            if (hitBoa.collider.CompareTag("ShootLine")) //Tiro a colonnello per entrambi
                            {
                                if ((player.idAnim == 0 && destination.y > player.transform.position.y) || (player.idAnim == 1 && destination.y < player.transform.position.y))
                                {
                                    player.LoadShoot(destination, shootFlag, 3);  // controllo se  davanti al portiere non può fare un colonnello all'indietro
                                }
                            }

                            else if (hitBoa.collider.CompareTag("Rovesciata"))

                            {
                                if (player.idAnim == 0 && destination.x <= player.transform.position.x && destination.y <= player.transform.position.y)
                                {   //il giocatore Y effettua un colonnello
                                 //   Debug.Log(name + "--->colonnello");
                                    player.LoadShoot(destination, shootFlag, 3);

                                }
                                else if (player.idAnim == 1 && destination.x >= player.transform.position.x && destination.y >= player.transform.position.y)
                                {
                                //    Debug.Log(name + "--->colonnello");
                                    player.LoadShoot(destination, shootFlag, 3);
                                }
                                else if (player.idTeam == 0 && destination.y > player.transform.position.y || player.idTeam == 1 && destination.y < transform.position.y)
                                {
                                    player.LoadShoot(destination, shootFlag, 1); //Rovesciata per entrambi
                                }
                            }

                            else if (hitBoa.collider.CompareTag("Sciarpa")) //sciarpa per entrambi a meno che la palla non è indirizzata in porta
                            {

                                if (player.idAnim == 0 && destination.x >= player.transform.position.x && destination.y <= player.transform.position.y)
                                {   //il giocatore Y effettua un colonnello
                                  //  Debug.Log(name + "--->colonnello");
                                    player.LoadShoot(destination, shootFlag, 3);

                                }   //il giocatore R effettua un colonnello
                                else if (player.idAnim == 1 && destination.x <= player.transform.position.x && destination.y >= player.transform.position.y)
                                {
                                //   Debug.Log(name + "--->colonnello");
                                    player.LoadShoot(destination, shootFlag, 3);
                                }
                                else if (player.idTeam == 0 && destination.y > player.transform.position.y || player.idTeam == 1 && destination.y < transform.position.y)
                                {

                                    player.LoadShoot(destination, shootFlag, 2);
                                }
                            }
                        }
                        else if (hitBoa.collider==null && player.idTeam == 0)
                        {

                            if (destination.y <= player.transform.position.y) //colonnello
                            {
                                player.LoadShoot(destination, shootFlag, 3);
                            }
                            else if (destination.y >= player.transform.position.y && destination.x <= player.transform.position.x) //Rovesciata
                            {
                                player.LoadShoot(destination, shootFlag, 1);

                            }
                            else if (destination.y >= player.transform.position.y && destination.x > player.transform.position.x) //sciarpa
                            {

                                player.LoadShoot(destination, shootFlag, 2);
                            }
                        }
                        else if (hitBoa.collider==null && player.idTeam == 1) //Boa Red
                        {
                            if (destination.y >= player.transform.position.y) //colonnello
                            {
                                player.LoadShoot(destination, shootFlag, 3);
                            }
                            if (destination.y <= player.transform.position.y && destination.x >= player.transform.position.x) //Rovesciata
                            {
                                player.LoadShoot(destination, shootFlag, 1);
                            }
                            if (destination.y <= player.transform.position.y && destination.x < player.transform.position.x) //sciarpa
                            {
                                player.LoadShoot(destination, shootFlag, 2);
                            }
                        }

                        
                    }


                    //Debug.Log(player.name + " ANNULLO IL TIRO");
                    player.DestroySignalShoot();
                    loadShoot = false;
                    player = null;
                    restart = true;


                }
                
                
               
            }
        }
    }


    

    public void Exit()
    {
        Application.Quit();
    }
}


