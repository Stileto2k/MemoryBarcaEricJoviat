using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq.Expressions;
using UnityEngine;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject[] cards;//Llista de referencies directes a les cartes com a objecte de referencia
    public GameObject[] cardSons;//Llista de referencies a les cartes com a render es a dir els fills dels objectes
    private double id_double=0; //Id per asignar parelles de cartes
    private GameObject[] cardsSelected = new GameObject[2]; //Llista de les dues cartes seleccionades
    private int cartesAdivinades;
    private bool clickTrigger=false; //Permis per clicar cartes.
    private float clickCooldown=0; //Un retras per activar el permis de clicar cartes
    private bool startGameTrigger=false; //Variable per donar pas a l'inici del joc
    public Button startButton;
    public TextMeshProUGUI timeText;
    private double timeNum; //Temps total
    private int intentsNum;
    public TextMeshProUGUI bestTime;
    public TextMeshProUGUI intentsText;
    public AudioClip[] audios; //Llista de audios
    public AudioSource audioSource; 


    // Start is called before the first frame update
    void Start()
    {
        //Al iniciar declarem el listener del boto de start.
        if (startButton != null)
        {
            startButton.onClick.AddListener(AccionBoton);
        }
        intentsNum=0;
        intentsText.text="Tries: "+intentsNum;
        bestTime.text="Best Time: "+PlayerPrefs.GetInt("BestScore", 0);//Recuperem el best score de la memoria
        timeText.text="Timer: "+0;
        cardsSelected[0]=null;
        cardsSelected[1]=null;
        cartesAdivinades=0;
        cards = GameObject.FindGameObjectsWithTag("CardTag");
        cardSons = GameObject.FindGameObjectsWithTag("CardTagSon");

    }

    // Update is called once per frame
    void Update()
    {
        //Temps de la partida
        if (startGameTrigger){
            timeNum+=Time.deltaTime;
            timeText.text=("Timer: "+(int)timeNum);
        }
        //Retras per poder pulsar alguna tecla
        if(clickTrigger){
            clickCooldown+=Time.deltaTime;
            if(clickCooldown>=1){
                clickCooldown=0;
                clickTrigger=false;
            }
        }
        //Comprova si hi ha dues cartes seleccionades
        if(cardsSelected[0]!=null && cardsSelected[1]!=null){
            //Si es aixi comprova que les cartes estiguin en el estat Up, mostrant la figura
            AnimatorStateInfo stateInfo0 = cardsSelected[0].gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo stateInfo1 = cardsSelected[1].gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            //Un cop estiguin Up comproven les ids...
            if(stateInfo0.IsName("FigureUp") && stateInfo1.IsName("FigureUp")){
                checkIds();
            }
        }
        //Si s'han adivinat 8 parelles llavors se acaba el joc.
        if(cartesAdivinades==8){
            audioSource.PlayOneShot(audios[1]);
            if(timeNum < PlayerPrefs.GetInt("BestScore", 0)){
                PlayerPrefs.SetInt("BestScore", (int)timeNum);
            }
            startGameTrigger=false;
            cartesAdivinades+=1;
            Invoke("FinishScene",5);
        }
    }

    //Funcio que cridem desde les cartes per avisar al GameManager que alguna ha estat apretada
    public void cardTouched(GameObject cardtouched){
        if(cardsSelected[0]==null && cardsSelected[1]==null){
            cardsSelected[0]=cardtouched.gameObject;
            audioSource.PlayOneShot(audios[0]);
        }else{
            cardsSelected[1]=cardtouched.gameObject;
            audioSource.PlayOneShot(audios[0]);
        }   
    }

    //Comprova les ids de les cartes aixecades per saber si son iguals o no.
    public void checkIds(){
        if(cardsSelected[0]!=null && cardsSelected[1]!=null){
            if(cardsSelected[0].GetComponent<CardScript>().getId()!=cardsSelected[1].GetComponent<CardScript>().getId()){
                cardsSelected[0].GetComponent<CardScript>().esconder();
                cardsSelected[1].GetComponent<CardScript>().esconder();
                intentsNum += 1;
                intentsText.text = "Tries: "+ intentsNum;
                audioSource.PlayOneShot(audios[3]);
                borrarSeleccionados(12);
            }else{
                cartesAdivinades++;
                audioSource.PlayOneShot(audios[4]);
                borrarSeleccionados(12);
            }
        }
    }

    //Serveix per saber si hi ha mes de 2 cartes seleccionades.
    public bool hiHaPuesto(){
        return cardsSelected[0]==null || cardsSelected[1]==null;
    }

    //Reset de les cartes selecionades.
    public void borrarSeleccionados(int num){
        if(num==0){
            cardsSelected[0]=null;
        }
        if(num==1){
            cardsSelected[1]=null;
        }
        if(num==12){
            cardsSelected[0]=null;
            cardsSelected[1]=null;
        }
    }

    //Barreja una llista de manera al random
    void Mezclar(GameObject[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int randomIndex = Random.Range(i, n);
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    //Cridem aquesta funcio quan apretem el boto de start, per fer el set de totes les variables.
    void AccionBoton()
    {
        audioSource.PlayOneShot(audios[2]);
        cardSons[0].GetComponent<CardScript>().setStartVar(true);
        startButton.gameObject.SetActive(false);
        startGameTrigger=true;
        intentsNum=0;
        intentsText.text="Intents: "+intentsNum;
        //Asignem ids i figures a les cartes.
        foreach(GameObject card in cardSons){
            card.GetComponent<CardScript>().setId(id_double);
            id_double=id_double+0.5;
            Material materialCargado = Resources.Load<Material>("Materials/FigureMaterial" + card.GetComponent<CardScript>().getId());
            card.GetComponent<CardScript>().getFiguraRenderer().material = materialCargado;
        }
        //Barrajem les cartes.
        Mezclar(cards);

        //Posicionem les cartes en el seu lloc.
        int i=0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Vector3 posicion = new Vector3((float)((x * 2)-2), (float)-1.3, (y * 2)-2);
                cards[i].transform.position = posicion;
                i++;
            }
        }
    }

    //Gracies a un Invoke fem reset a la escena per tornar a jugar.
    void FinishScene(){
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    /*Setters i getters*/
    public bool canClickTrigger(){
        return !clickTrigger;
    }

    public void setClickTrigger(bool clickTriger){
        clickTrigger = clickTriger;
    }
}