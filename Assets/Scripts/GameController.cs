using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.IO;

public class GameController : MonoBehaviour
{
  public string wsURL = "ws://193.137.46.11/";
  public string appName = "Fanima"; 
  public int therapistID;
  const int PLAYGAMEID = 29;
  public int patientID;
  public bool structReqDone = false;
  public bool respositoryReqDone = false;
  public bool gameExecutionDone = false;
  public bool therapistReady = false;
  public bool errorDone = false;
  public int gameExecutionID = -1;
  public int gameSampleID = -1;
  public List<actionClass> contentList;
  public List<dataSource> dataList;
  
  public WebRequests webRequests;
  public WebSockets webSockets;
  
  private Rigidbody2D rb;

  [SerializeField] private ParticleSystem confetti;
  public GameObject finalMenu;
    
  [SerializeField] private AudioSource userRecording;
  
  public GameObject startMenuUI;

  public GeralScript geralScript;
  public ChameleonScript chameleonScript;
  public FrogScript frogScript;
  public TravelScript travelScript;
  public MonkeyScript monkeyScript;
  public OwlScript owlScript;
  public OctopusScript octopusScript;
  public FishScript fishScript;

  string startTime;
  string endTime;

  string activeChapter = "";
  public List<actionClass> sequenceToPlayList;
  public List<string> listOfWordsToSay; 
  public string currentWord;
  public int currentActionID = -1;
  public int currentWordID = -1;
  public int currentSequenceID = -1;
  public int currentLevelID = -1;
  public int sequenceID = -1;
  public int timer = -1;
  public int errorStatus = -1;
  public int repSampleID = -1;

  public bool validationDone = false;
  public bool selectionDone;
  public bool postGameResultDone = false;
  public bool errorDetected = false;
  public bool readyForNextWord = false;
  public bool prepareLevelsDone = false; 
  public bool repetition = false;
  public bool bonusgameResult = false;
  public bool actionValidated = false;
  public bool lastBonusSample = false;

  public bool speak = false;
  public bool activeHelpButton = false;
  public bool requestTherapistStatus = false;
  public bool startMicro = false;

  public List<errorClass> phonemeList;

  public AudioSource aud;
  public List<AudioClip> wordsNameClips;
  public List<AudioClip> clips;

  public AudioSource introChapVoice;
  public AudioSource finalChapVoice;
  public AudioSource rewardVoice;
  public AudioSource finalSentencesChapVoice;
  public AudioSource travelTrip1;
  public AudioSource travelTrip2;
  public AudioSource travelTrip3;
  public AudioSource travelFinal;

  public int numberToDivideProcessBar = -1;
  public string levelsJson = "";

  public List<string> levels; 

  public bool adjustIncrementAmountDone = false;
  
  public GameObject finalRewardBoard;
  public GameObject finalReward0;
  public GameObject finalReward0S;
  public GameObject finalReward1;
  public GameObject finalReward1E;
  public GameObject finalReward2;
  public GameObject finalReward2E;
  public GameObject finalReward3;
  public GameObject finalReward3E;

  public List<string> lvlsRestore; 
  public bool responseToRestoreDone = false;

  public bool requestGameExecutionID = false;

  // Start is called before the first frame update
  void Start()
  {
    if(SceneManager.GetActiveScene().name == "Stop")
    {
      StartCoroutine(StopSession());
    }

    if(SceneManager.GetActiveScene().name == "Travel")
    {
      finalMenu.SetActive(false);
      
      finalRewardBoard.SetActive(false);
      finalReward0.SetActive(false);
      finalReward0S.SetActive(false);
      finalReward1.SetActive(false);
      finalReward1E.SetActive(false);
      finalReward2.SetActive(false);
      finalReward2E.SetActive(false);
      finalReward3.SetActive(false);
      finalReward3E.SetActive(false);
    }
    
    aud = GetComponent<AudioSource>();

    rb = GetComponent<Rigidbody2D>();
    webSockets = new WebSockets();
    therapistID = PlayerPrefs.GetInt("THERAPISTID");
    webSockets.therapistID = therapistID;
    patientID = PlayerPrefs.GetInt("PATIENTID");
    webSockets.SetupClient(wsURL, patientID, PLAYGAMEID, appName);
    webSockets.StartClient();

    List<string> listOfWordsToSay = new List<string>(); 
    List<actionClass> sequenceToPlayList = new List<actionClass>(); 
    List<errorClass> phonemeList = new List<errorClass>(); 

    if(SceneManager.GetActiveScene().name == "Geral")
    {
      activeChapter = "Geral"; 
      PlayerPrefs.SetString("LEVELSELECTION", "NOTDONE");  
      PlayerPrefs.SetInt("ChapterPlayed", 0);  
      StartCoroutine(GameLoop());    
    }

    else if(SceneManager.GetActiveScene().name == "Travel")
    {      
      if(PlayerPrefs.GetString("LEVELSELECTION").Equals("NOTDONE"))
      { 
        StartCoroutine(PrepareLevels());
      }
      StartCoroutine(PrepareNextLevel());
    }
      
    else if(SceneManager.GetActiveScene().name == "Frog")
    {
      activeChapter = "Oclusivas";     
      StartCoroutine(GameLoop()); 
    }

    else if(SceneManager.GetActiveScene().name == "Owl")
    {
      activeChapter = "Fricativas";      
      StartCoroutine(GameLoop());
    }

    else if(SceneManager.GetActiveScene().name == "Fish")
    { 
      activeChapter = "Vibrantes e Laterais";
      StartCoroutine(GameLoop());      
    }

    else if(SceneManager.GetActiveScene().name == "Monkey")
    {
      StartCoroutine(BonusGameLoop());
    }

    else if(SceneManager.GetActiveScene().name == "Chameleon")
    {
      StartCoroutine(BonusGameLoop());
    }

    else if(SceneManager.GetActiveScene().name == "Octopus") 
    {
      StartCoroutine(BonusGameLoop());
    }
  }

  void Update()
  {
    if(PlayerPrefs.GetString("LEVELSELECTION").Equals("DONE"))
    {
      selectionDone = true;
    }
    else
    {
      selectionDone = false;
    }
    if(requestTherapistStatus)
    {
      RequestTherapistStatus();
      requestTherapistStatus = false;
    }
     if(webSockets.stop)
     {
      webSockets.stop = false;
      //INTERROMPER JOGO
      Debug.Log("JOGO INTERROMPIDO!");
      SceneManager.LoadScene("Stop");
     }
  }

  public void RequestTherapistStatus()
  {
    StartCoroutine(CheckTherapistStatus());
  }

  IEnumerator CheckTherapistStatus()
  {
    yield return new WaitUntil(() => webSockets.socketIsReady);
    webSockets.VerifyTherapistActivity(therapistID);
    yield return new WaitUntil(() => webSockets.getAwareValue);
    Debug.Log("AWARE VALUE " + webSockets.awareValue);
    if(webSockets.awareValue == 1)
    {
      therapistReady = true;
    }
    else
    {
      therapistReady = false;
    }
  }

  IEnumerator GameLoop()
  {
    if((SceneManager.GetActiveScene().name == "Geral"))
    {
      //FAZER PEDIDO DE RESTORE (RESTORE == 1 CASO JA SE TENHA FEITO)
      if(PlayerPrefs.GetInt("RESTORE") != 1)
      {
        Debug.Log("1 não envia pedido / 0 envia: " + PlayerPrefs.GetInt("RESTORE"));
        yield return new WaitUntil(() => therapistReady);
        
        webSockets.RestoreRequest(therapistID);
        yield return new WaitUntil(() => webSockets.socketIsReady);
        yield return new WaitUntil(() => webSockets.restoreDone);
        responseToRestoreDone = true;
      }
      yield return StartCoroutine(PreparedGameExecutionID());
      yield return StartCoroutine(GeralIntro());
      geralScript.doAnimation = true;
    }

    yield return StartCoroutine(ChapIntroVoices());
    yield return StartCoroutine(PrepareSequence());
    yield return new WaitUntil(() => sequenceToPlayList.Count > 0);

    
    for(int i = 0; i < sequenceToPlayList.Count; i++)
    {
      currentActionID = sequenceToPlayList[i].id;
      currentWordID = sequenceToPlayList[i].word;
      currentLevelID = sequenceToPlayList[i].levelid;
      PlayerPrefs.SetInt("SEQUENCEID", sequenceToPlayList[i].sequenceid);
      startTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
      FindWordNameByWordId(currentWordID);
      string payload = "{\"therapist\": " + therapistID + ", \"game\": \"" + PLAYGAMEID + "\", \"execution\": \"" + gameExecutionID + "\", \"status\": " + 0 + ", \"order\": " + 0 + ", \"level\": \"" + sequenceToPlayList[i].level + "\", \"sequence\": \"" + sequenceToPlayList[i].sequence + "\", \"action\": \"" + sequenceToPlayList[i].id + "\", \"percent\": " + 0 + ", \"time\": " + 0 + "}";        
      webSockets.PrepareMessage("game", payload); 
      yield return StartCoroutine(PlaySentences(currentWord));
      Debug.Log("DIZ -> " + currentWord);
      ShowImage(currentWord);
      
      if((SceneManager.GetActiveScene().name == "Geral"))
      {
        yield return new WaitUntil(() => geralScript.animationDone);
        if(geralScript.wordsDone == false)
        {
          //PALAVRAS
          yield return new WaitUntil(() => geralScript.startValidation);
        }
        else
        {
          //FRASES
          yield return new WaitUntil(() => geralScript.animationDone);
          yield return StartCoroutine(PlayGuideVoice(currentWord));
          geralScript.startValidation = true;
          
        }
      }

      if((SceneManager.GetActiveScene().name == "Owl"))
      {
        yield return new WaitUntil(() => owlScript.pop);
        yield return new WaitForSeconds(1.0f);
      }

      timer = sequenceToPlayList[i].time;

      if(repetition == false)
      {
        //tempo de perceber o que é a imagem
        yield return new WaitForSeconds(1.0f);
        speak = true;
      }
      speak = true;
      activeHelpButton = true;
      yield return new WaitUntil(() => startMicro);
      startMicro = false;
      RecordSound(timer);
      yield return StartCoroutine(WaitForValidation());
    }
    yield return StartCoroutine(PlayAudioClip("chapEndMusic"));
    ShowReward();
    yield return StartCoroutine(ChapFinalVoices());
    Debug.Log("ACABOU O SEQUENCIA");

    if((SceneManager.GetActiveScene().name == "Geral"))
    {
      SceneManager.LoadScene("Travel"); 
    }

    else if((SceneManager.GetActiveScene().name == "Frog"))
    {
      if(errorDetected == true)
      {
        SceneManager.LoadScene("Monkey"); 
      }
      else
      {
        SceneManager.LoadScene("Travel"); 
      }
    }

    else if((SceneManager.GetActiveScene().name == "Owl"))
    {
      if(errorDetected == true)
      {
        SceneManager.LoadScene("Chameleon"); 
      }
      else
      {
        SceneManager.LoadScene("Travel"); 
      }
    }

    else if((SceneManager.GetActiveScene().name == "Fish"))
    {
      if(errorDetected == true)
      {
        SceneManager.LoadScene("Octopus"); 
      }
      else
      {
        SceneManager.LoadScene("Travel"); 
      }
    }
  }

  public void ShowReward()
  {
    if((SceneManager.GetActiveScene().name == "Geral"))
    {
      geralScript.showSentencesReward = true;
    }

    else if((SceneManager.GetActiveScene().name == "Frog"))
    {
      frogScript.showReward = true;
    }

    else if((SceneManager.GetActiveScene().name == "Owl"))
    {
      owlScript.showReward = true;
    }

    else if((SceneManager.GetActiveScene().name == "Fish"))
    {
      fishScript.showReward = true;
    }
  }

  public void ShowRewardBonusLevels()
  {
    if((SceneManager.GetActiveScene().name == "Monkey"))
    {
      monkeyScript.showReward = true;
    }

    else if((SceneManager.GetActiveScene().name == "Chameleon"))
    {
      chameleonScript.showReward = true;
    }

    else if((SceneManager.GetActiveScene().name == "Octopus"))
    {
      octopusScript.showReward = true;
    }
  }


  IEnumerator BonusGameLoop()
  {
    yield return StartCoroutine(ChapIntroVoices());
    gameExecutionID = PlayerPrefs.GetInt("GAMEEXECUTIONID");
    sequenceID = PlayerPrefs.GetInt("SEQUENCEID");
    yield return StartCoroutine(webRequests.GetChapterErrors(gameExecutionID.ToString(), sequenceID.ToString()));
    yield return new WaitUntil(() => webRequests.chapterErrorListDone);
    
    AdjustIncrementAmount();
    yield return new WaitUntil(() => adjustIncrementAmountDone);

    for(int i = 0; i < webRequests.chapterErrorList.Count; i ++)
    { 
      
      activeChapter = "Fonema /" + webRequests.chapterErrorList[i].phoneme + "/";
      Debug.Log("FONEMA: " + activeChapter);
      
      yield return StartCoroutine(PrepareSequence());
      yield return new WaitUntil(() => sequenceToPlayList.Count > 0);
      Debug.Log("NUMERO DE PALAVRAS " + sequenceToPlayList.Count);
      for(int j = 0; j < sequenceToPlayList.Count; j++)
      { 
        currentActionID = sequenceToPlayList[j].id;
        currentWordID = sequenceToPlayList[j].word;
        currentSequenceID = sequenceToPlayList[j].sequenceid;
        currentLevelID = sequenceToPlayList[j].levelid;

        startTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        FindWordNameByWordId(currentWordID);
        string payload = "{\"therapist\": " + therapistID + ", \"game\": \"" + PLAYGAMEID + "\", \"execution\": \"" + gameExecutionID + "\", \"status\": " + 0 + ", \"order\": " + 0 + ", \"level\": \"" + sequenceToPlayList[j].level + "\", \"sequence\": \"" + sequenceToPlayList[j].sequence + "\", \"action\": \"" + sequenceToPlayList[j].id + "\", \"percent\": " + 0 + ", \"time\": " + 0 + "}";        
        webSockets.PrepareMessage("game", payload); 
     
        //Repeat the same word 3 times
        for(int l = 0; l < 3; l++)
        {
          if(l == 2)
          {
            lastBonusSample = true;
            yield return StartCoroutine(PlayGuideVoiceForReps(l));
          }
          bonusgameResult = true;
          ShowImageBonus(currentWord, l);
          if(l == 0)
          {
            //tempo de perceber o que é a imagem
            yield return new WaitForSeconds(1.0f);
            speak = true;
          }
          if(l == 1)
          {
            yield return StartCoroutine(PlayGuideVoiceForReps(l));
          }
          timer = sequenceToPlayList[j].time;

          activeHelpButton = true;
          speak = true;
          yield return new WaitUntil(() => startMicro);
          startMicro = false;
          RecordSound(timer);
          yield return StartCoroutine(WaitForValidation());
        }
      }   
    }
    yield return StartCoroutine(PlayAudioClip("chapEndMusic"));
    ShowRewardBonusLevels();
    yield return StartCoroutine(ChapFinalVoices());

    Debug.Log("ACABOU O SEQUENCIA"); 
    SceneManager.LoadScene("Travel");
  }

  public void FindWordNameByWordId(int wordID)
  {
    for(int i = 0; i < dataList.Count; i ++)
    {
      if(dataList[i].id == wordID)
      {
        currentWord = dataList[i].name;
      }
    }
  }

  public void AdjustIncrementAmount()
  {
    if((SceneManager.GetActiveScene().name == "Monkey"))
    { 
      bool addThree = false;
      for(int i = 0; i < webRequests.chapterErrorList.Count; i ++)
      {
        if(webRequests.chapterErrorList[i].phoneme == "nh")
        {
          addThree = true;
        }      
      }
      
      if(addThree)
      {
        Debug.Log("PHONEMES COUNT: " + webRequests.chapterErrorList.Count);
        float minusPhonemeNh = (float)webRequests.chapterErrorList.Count - (float)1;
        Debug.Log("NO NH COUNT: " + minusPhonemeNh);
        monkeyScript.incrementAmount = (float)1 / (minusPhonemeNh * (float)5 + (float)3);
        Debug.Log("BAR INCREMENT COM NH: " + monkeyScript.incrementAmount);

      }
      else
      {
        monkeyScript.incrementAmount = (float)1 / ((float)webRequests.chapterErrorList.Count * (float)5);
        Debug.Log("BAR INCREMENT: " + monkeyScript.incrementAmount);

      }
    }

    else if((SceneManager.GetActiveScene().name == "Chameleon"))
    {
      chameleonScript.incrementAmount = (float)1 / ((float)webRequests.chapterErrorList.Count * (float)5);
      Debug.Log("BAR INCREMENT: " + chameleonScript.incrementAmount);

    }

    else if((SceneManager.GetActiveScene().name == "Octopus"))
    {
      octopusScript.incrementAmount = (float)1 / ((float)webRequests.chapterErrorList.Count * (float)5);
      Debug.Log("BAR INCREMENT: " + octopusScript.incrementAmount);
    }
    adjustIncrementAmountDone = true;
  }

  public void ShowImageBonus(string currentWord, int repNumber)
  {
    if((SceneManager.GetActiveScene().name == "Monkey"))
    {
      monkeyScript.currentWord = currentWord;
      monkeyScript.canShowImage = true;
      monkeyScript.repNumber = repNumber;
    }
    else if((SceneManager.GetActiveScene().name == "Chameleon"))
    {
      chameleonScript.currentWord = currentWord;
      chameleonScript.canShowImage = true;
      chameleonScript.repNumber = repNumber;
    }
    else if((SceneManager.GetActiveScene().name == "Octopus"))
    {
      octopusScript.currentWord = currentWord;
      octopusScript.canShowImage = true;
      octopusScript.repNumber = repNumber;
    }
  }

  public void ShowImage(string currentWord)
  {
    if((SceneManager.GetActiveScene().name == "Geral"))
    {
      geralScript.currentWord = currentWord;
      geralScript.canShowImage = true;
    }
    else if((SceneManager.GetActiveScene().name == "Frog"))
    {
      frogScript.currentWord = currentWord;
      frogScript.canShowImage = true;
    }
    else if((SceneManager.GetActiveScene().name == "Owl"))
    {
      owlScript.currentWord = currentWord;
      owlScript.canShowImage = true;
    }
    else if((SceneManager.GetActiveScene().name == "Fish"))
    {
      fishScript.currentWord = currentWord;
      fishScript.canShowImage = true;
    }
  }

  IEnumerator GeralIntro()
  {
    yield return new WaitUntil(() => PlayerPrefs.GetInt("GAMESTARTED") == 1);
    yield return StartCoroutine(PlayAudioClip("Intro words"));
  }

  IEnumerator ChapIntroVoices()
  {
    introChapVoice.Play();
    if(SceneManager.GetActiveScene().name == "Frog")
    {
      yield return new WaitForSeconds(6.7f);
    }
    else if (SceneManager.GetActiveScene().name == "Monkey")
    {
      yield return new WaitForSeconds(13.7f);
    }
    else if (SceneManager.GetActiveScene().name == "Owl")
    {
      yield return new WaitForSeconds(7.4f);
    }
    else if (SceneManager.GetActiveScene().name == "Chameleon")
    {
      yield return new WaitForSeconds(17.2f);
    }
    else if (SceneManager.GetActiveScene().name == "Fish")
    {
      yield return new WaitForSeconds(6.5f);    
    }
    else if (SceneManager.GetActiveScene().name == "Octopus")
    {
      yield return new WaitForSeconds(13.9f);
    }
  }

  IEnumerator ChapFinalVoices()
  {
    finalChapVoice.Play();
    if(SceneManager.GetActiveScene().name == "Geral")
    {
      PlayerPrefs.SetInt("Chap0S", 1);
      yield return new WaitForSeconds(4.0f);
    }
    else if(SceneManager.GetActiveScene().name == "Frog")
    {
      PlayerPrefs.SetInt("Chap1", 1);
     yield return new WaitForSeconds(4.0f);
    }
    else if (SceneManager.GetActiveScene().name == "Monkey")
    {
      PlayerPrefs.SetInt("Chap1E", 1);
      yield return new WaitForSeconds(5.5f);
    }
    else if (SceneManager.GetActiveScene().name == "Owl")
    {
      PlayerPrefs.SetInt("Chap2", 1);
      yield return new WaitForSeconds(5.0f);
    }
    else if (SceneManager.GetActiveScene().name == "Chameleon")
    {
      PlayerPrefs.SetInt("Chap2E", 1);
      yield return new WaitForSeconds(5.5f);
    }
    else if (SceneManager.GetActiveScene().name == "Fish")
    {
      PlayerPrefs.SetInt("Chap3", 1);
      yield return new WaitForSeconds(5.5f);
    }
    else if (SceneManager.GetActiveScene().name == "Octopus")
    {
      PlayerPrefs.SetInt("Chap3E", 1);
      yield return new WaitForSeconds(5.5f);
    }
    rewardVoice.Play();
    yield return new WaitForSeconds(2.5f);

  }

  IEnumerator PlaySentences(string currentWord)
  {
    if(currentWord == "O sapato da menina tem bolas amarelas.")
    {
      geralScript.wordsDone = true;
      webSockets.PlaySentencesRequest(therapistID);
      //webSockets.playSentences = 1 playSentences     webSockets.playSentences = -1 dont playSentences
      yield return new WaitUntil(() => webSockets.getPlaySentencesDone);
      if(webSockets.playSentences == 1)
      {
        //barra de progresso volta a zero
        //incremeto da barra alterado mediante o numeor de frases a jogar
        geralScript.barImage.fillAmount = 0.0f;
        geralScript.incrementAmount = 0.17f;
        

        yield return StartCoroutine(PlayAudioClip("Intro sentences"));
        geralScript.doAnimation = true;

      }
      else if(webSockets.playSentences == -1)
      {
        //Não jogar frases 
        //yield return StartCoroutine(PlayAudioClip("chapEndMusic"));
        geralScript.showWordsReward = true;
        PlayerPrefs.SetInt("Chap0", 1);
        yield return StartCoroutine(PlayAudioClip("finalWords"));
        SceneManager.LoadScene("Travel");
      } 
    }
  }

  IEnumerator PlayWordName(string clipToPlay)
  {
    foreach (AudioClip clip in wordsNameClips)
    {
      if(clip.name == clipToPlay)
      {
        aud.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
      }
    }
  }

  IEnumerator PrepareNextLevel()
  {    
    yield return new WaitUntil(() => selectionDone);
    if(PlayerPrefs.GetInt("ChapterPlayed") == 0)
    {
      PlayerPrefs.SetInt("ChapterPlayed", 1);
      travelTrip1.Play();
      yield return new WaitForSeconds(4.0f);
      SceneManager.LoadScene(PlayerPrefs.GetString("ChapterOne"));
    }
    else if(PlayerPrefs.GetInt("ChapterPlayed") == 1)
    {
      PlayerPrefs.SetInt("ChapterPlayed", 2);
      if(PlayerPrefs.GetInt("ChaptersToQuitGame") == 1)
      {
        confetti.Play();
        travelFinal.Play();
        ShowFinalRewards();
        yield return new WaitForSeconds(5.0f);
        finalMenu.SetActive(true);
      }
      else
      {
        travelTrip2.Play();
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(PlayerPrefs.GetString("ChapterTwo")); 
      }
    }
    else if(PlayerPrefs.GetInt("ChapterPlayed") == 2)
    {
      if(PlayerPrefs.GetInt("ChaptersToQuitGame") == 2)
      {
        confetti.Play();
        travelFinal.Play();
        ShowFinalRewards();
        yield return new WaitForSeconds(5.0f);
        finalMenu.SetActive(true);
      }
      else
      {
        PlayerPrefs.SetInt("ChapterPlayed", 3);
        travelTrip3.Play();
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(PlayerPrefs.GetString("ChapterThree")); 
      }
    }
    else if(PlayerPrefs.GetInt("ChapterPlayed") == 3)
    {
       if(PlayerPrefs.GetInt("ChaptersToQuitGame") == 3)
      {
        confetti.Play();
        travelFinal.Play();
        ShowFinalRewards();
        yield return new WaitForSeconds(5.0f);
        finalMenu.SetActive(true);
      }
    }
  }

  public void ShowFinalRewards()
  {
    finalRewardBoard.SetActive(true);
    if(PlayerPrefs.GetInt("Chap0") == 1)
    {
      finalReward0.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap0S") == 1)
    {
      finalReward0S.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap1") == 1)
    {
      finalReward1.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap1E") == 1)
    {
      finalReward1E.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap2") == 1)
    {
      finalReward2.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap2E") == 1)
    {
      finalReward2E.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap3") == 1)
    {
      finalReward3.SetActive(true);
    }
    if(PlayerPrefs.GetInt("Chap3E") == 1)
    {
      finalReward3E.SetActive(true);
    }

    if (PlayerPrefs.HasKey("Chap0"))
    {
      PlayerPrefs.DeleteKey("Chap0");
    }
    if (PlayerPrefs.HasKey("Chap0S"))
    {
      PlayerPrefs.DeleteKey("Chap0S");
    }
    if (PlayerPrefs.HasKey("Chap1"))
    {
      PlayerPrefs.DeleteKey("Chap1");
    }
    if (PlayerPrefs.HasKey("Chap1E"))
    {
      PlayerPrefs.DeleteKey("Chap1E");
    }
    if (PlayerPrefs.HasKey("Chap2"))
    {
      PlayerPrefs.DeleteKey("Chap2");
    }
    if (PlayerPrefs.HasKey("Chap2E"))
    {
      PlayerPrefs.DeleteKey("Chap2E");
    }
    if (PlayerPrefs.HasKey("Chap3"))
    {
      PlayerPrefs.DeleteKey("Chap3");
    }
    if (PlayerPrefs.HasKey("Chap3E"))
    {
      PlayerPrefs.DeleteKey("Chap3E");
    }
  }

  IEnumerator PrepareSequence()
  {
    if(sequenceToPlayList.Count > 0 )
    {
      sequenceToPlayList.Clear();
    }
    Debug.Log("Esperar pela estrutura...");
    yield return StartCoroutine(PrepareGameStructure());
    for(int i = 0; i < contentList.Count; i++)
    {
      if(contentList[i].sequence == activeChapter)
      {
        sequenceToPlayList.Add(contentList[i]);
      } 
    }
    Debug.Log("estrutura DONE");
  }

  IEnumerator PrepareLevels()
  {
    if (PlayerPrefs.HasKey("ChapterOne"))
    {
      PlayerPrefs.DeleteKey("ChapterOne");
    }
    if (PlayerPrefs.HasKey("ChapterTwo"))
    {
      PlayerPrefs.DeleteKey("ChapterTwo");
    }
    if (PlayerPrefs.HasKey("ChapterThree"))
    {
      PlayerPrefs.DeleteKey("ChapterThree");
    }
   //pede para escolher os niveis caso a lista esteja vazia
   if(PlayerPrefs.GetInt("CONTINUEGAME") == 1)
   {
     if(PlayerPrefs.GetInt("LASTLVLPLAYED") == 0)
      {
        yield return new WaitUntil(() => webSockets.socketIsReady);
        webSockets.LevelsToPlayRequest(therapistID);
        yield return new WaitUntil(() => webSockets.getLevelsDone);
      }
   }
    //webSockets.levelsList = lvlsRestore;

    PlayerPrefs.SetString("LEVELSELECTION", "DONE");
    for (int i = 0; i < webSockets.levelsList.Count; i++)
    {
      levels.Add(webSockets.levelsList[i]);
      Debug.Log("LVL: " + webSockets.levelsList[i]);
    }

    levelsJson = JsonUtility.ToJson(levels);

    if(webSockets.levelsList.Count == 1)
    {
      PlayerPrefs.SetInt("ChaptersToQuitGame", 1);
      PlayerPrefs.SetInt("NumberOfChaptersToPlay", 1);

      if(webSockets.levelsList[0].Equals("1"))
      {
        PlayerPrefs.SetString("ChapterOne", "Frog");

      }
      else if(webSockets.levelsList[0].Equals("2"))
      {
        PlayerPrefs.SetString("ChapterOne", "Owl");
      }
      else if(webSockets.levelsList[0].Equals("3"))
      {
        PlayerPrefs.SetString("ChapterOne", "Fish");
        
      }
    }
    else if(webSockets.levelsList.Count == 2)
    {
      PlayerPrefs.SetInt("ChaptersToQuitGame", 2);
      PlayerPrefs.SetInt("NumberOfChaptersToPlay", 2);
      if(webSockets.levelsList[0].Equals("1"))
      {
        PlayerPrefs.SetString("ChapterOne", "Frog");

        if(webSockets.levelsList[1].Equals("2"))
        {
          PlayerPrefs.SetString("ChapterTwo", "Owl");
        }
        else if(webSockets.levelsList[1].Equals("3"))
        {
          PlayerPrefs.SetString("ChapterTwo", "Fish");
        }
      }
      else if(webSockets.levelsList[0].Equals("2"))
      {
        PlayerPrefs.SetString("ChapterOne", "Owl");
        PlayerPrefs.SetString("ChapterTwo", "Fish");
      }
    }
    else if(webSockets.levelsList.Count == 3)
    {
      PlayerPrefs.SetInt("ChaptersToQuitGame", 3);
      PlayerPrefs.SetInt("NumberOfChaptersToPlay", 3);
      PlayerPrefs.SetString("ChapterOne", "Frog");
      PlayerPrefs.SetString("ChapterTwo", "Owl");
      PlayerPrefs.SetString("ChapterThree", "Fish");
    }
    PlayerPrefs.SetString("LEVELSELECTION", "DONE");
  prepareLevelsDone = true;
  }

  IEnumerator WaitForValidation()
  {
    yield return new WaitUntil(() => webSockets.socketIsReady);
    
    if (SceneManager.GetActiveScene().name == "Geral")
    {
      //Esperar pelo click no ramo
      yield return new WaitUntil(() => geralScript.startValidation);

      yield return new WaitForSeconds(timer);

      endTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"); 
      speak = false;        
      SavWav.Save(currentWord + ".wav", userRecording.clip);

      gameExecutionID = PlayerPrefs.GetInt("GAMEEXECUTIONID");
       
      if(repetition == true)
      {
        yield return StartCoroutine(webRequests.PostRepSample(currentWord, currentActionID.ToString(), gameExecutionID.ToString(), currentWordID.ToString(), repSampleID.ToString()));   
      }
      else
      {
        yield return StartCoroutine(webRequests.PostSample(currentWord, currentActionID.ToString(), gameExecutionID.ToString(), currentWordID.ToString()));
        gameSampleID = PlayerPrefs.GetInt("GAMESAMPLEID");
        Debug.Log("SAMPLE ID " + gameSampleID);
        yield return StartCoroutine(webRequests.PostGameRequest(gameSampleID.ToString()));
      }
      webSockets.ActionClassificationGeralRequest(therapistID, currentWordID, gameSampleID);
    }
    else
    {
      //Esperar pelo click no balao
      if (SceneManager.GetActiveScene().name == "Owl")
      {
        yield return new WaitUntil(() => owlScript.pop);
      }
      yield return new WaitForSeconds(timer);
      endTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");  
      speak = false;       
      SavWav.Save(currentWord + ".wav", userRecording.clip);

      gameExecutionID = PlayerPrefs.GetInt("GAMEEXECUTIONID");
       
      if(repetition == true)
      {
        yield return StartCoroutine(webRequests.PostRepSample(currentWord, currentActionID.ToString(), gameExecutionID.ToString(), currentWordID.ToString(), repSampleID.ToString()));   
      }
      else
      {
        yield return StartCoroutine(webRequests.PostSample(currentWord, currentActionID.ToString(), gameExecutionID.ToString(), currentWordID.ToString()));
        gameSampleID = PlayerPrefs.GetInt("GAMESAMPLEID");

        yield return StartCoroutine(webRequests.PostGameRequest(gameSampleID.ToString()));
      }
      webSockets.ActionClassificationRequest(therapistID, currentWordID, gameSampleID);
    }
    //ESPERAR ATE QUE A VALIDACAO SEJA FEITA
    yield return new WaitUntil(() => webSockets.validationDone);
    yield return new WaitUntil(() => webSockets.validationValue > -3);
    
    //HELP
    if(webSockets.validationValue == -2)
    {
      yield return StartCoroutine(PlayAudioClip("help"));
      yield return StartCoroutine(PlayWordName(currentWord));
      Debug.Log("Repete comigo, " + currentWord);
      startTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
      
      speak = true;

      RecordSound(timer);
      webSockets.validationValue = -3;
      repetition = true;
      repSampleID = gameSampleID;
      yield return StartCoroutine(WaitForValidation());
    }
    //REPITION
    else if(webSockets.validationValue == -1)
    {
      yield return StartCoroutine(PlayAudioClip("repeat"));
      Debug.Log("Não percebi, podores repetir? "); 
      startTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
      
      speak = true;

      RecordSound(timer);
      webSockets.validationValue = -3;
      repetition = true;
      repSampleID = gameSampleID;
      yield return StartCoroutine(WaitForValidation());
    }

    else if(webSockets.validationValue >= 0)
    {
      if(repetition == true)
      {     
        repetition = false;
      }

      if(webSockets.validationValue > 0)
      {
        errorDetected = true;
      }
      //desativar ajuda
      activeHelpButton = false;
      if(SceneManager.GetActiveScene().name == "Geral")
      {
        if(currentWord == "caracol")
        {
          yield return StartCoroutine(PlayAudioClip("validationMusic"));
          geralScript.doAnimation = false;
        }
        else if(currentWord == "O caracol dorme ao sol.")
        {
          geralScript.barImage.fillAmount += geralScript.incrementAmountSentences;
          geralScript.doAnimation = false;
        } 
        else
        {
          yield return StartCoroutine(PlayAudioClip("validationMusic"));
          if(geralScript.wordsDone)
          {
            geralScript.barImage.fillAmount += geralScript.incrementAmountSentences;
            geralScript.doAnimation = true;
            geralScript.animationDone = false;
          }
          else
          {
            geralScript.doAnimation = true;
            geralScript.animationDone = false;
          }
          
        }
          webSockets.validationValue = -3;  
           
      }
      else if (SceneManager.GetActiveScene().name == "Frog")
      {
        frogScript.validationDone = true;
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        frogScript.canShake = true;
        yield return StartCoroutine(PlayAudioClip("touchCoin"));
        yield return new WaitUntil(() => frogScript.isCaught);
        frogScript.isCaught = false;
        webSockets.validationValue = -3;
      }
      else if (SceneManager.GetActiveScene().name == "Owl")
      {
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        owlScript.pop = false;
        owlScript.nextAction = true;
        webSockets.validationValue = -3;
      }
      else if (SceneManager.GetActiveScene().name == "Octopus")
      {
        if(lastBonusSample == true)
        {
          octopusScript.repNumber = 3;
          yield return StartCoroutine(PlayAudioClip("matchOctopus"));
          yield return new WaitUntil(() => octopusScript.isMatch);
          octopusScript.isMatch = false;
        }
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        octopusScript.nextAction = true;
        webSockets.validationValue = -3;   
      }
      else if (SceneManager.GetActiveScene().name == "Monkey")
      {
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        if(lastBonusSample)
        {
          monkeyScript.newWord = true;
          monkeyScript.randomIndex = Random.Range(0, 8);
          yield return StartCoroutine(PlayAudioClip("findMonkey"));
          yield return new WaitUntil(() => monkeyScript.isCaught);
        }
        monkeyScript.nextAction = true;
        webSockets.validationValue = -3; 
     
        }
      else if (SceneManager.GetActiveScene().name == "Chameleon")
      {
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        if(lastBonusSample)
        {
          chameleonScript.newWord = true;
          //chameleonScript.nextAction = true; 
          chameleonScript.randomIndex = Random.Range(0, 13);
          yield return StartCoroutine(PlayAudioClip("findChameleon"));
          yield return new WaitUntil(() => chameleonScript.isCaught);
        }
        chameleonScript.nextAction = true; 
        webSockets.validationValue = -3;
      
      }
      else if (SceneManager.GetActiveScene().name == "Fish")
      {
        yield return StartCoroutine(PlayAudioClip("validationMusic"));
        fishScript.canShake = true;
        yield return StartCoroutine(PlayAudioClip("touchFood"));
        yield return new WaitUntil(() => fishScript.isCaught);
        fishScript.isCaught = false;
        webSockets.validationValue = -3;
     
      }
      yield return StartCoroutine(PreparedGameResult());
    }
  }

  IEnumerator PrepareGameStructure()
  {
    yield return new WaitUntil(() => structReqDone);
    yield return new WaitUntil(() => respositoryReqDone);
  }

  IEnumerator PreparedGameExecutionID()
  {
    if (SceneManager.GetActiveScene().name == "Geral")
    {
      yield return new WaitUntil(() => webSockets.restoreDone);
      Debug.Log("RESPOSTA RESTORE: " + webSockets.restoreGameExecutionID);

      if(webSockets.continueGame)
      {
        //em caso de restore
        PlayerPrefs.SetInt("RESTORE", 1);
        Debug.Log("CONTINUAR JOGO!");
        PlayerPrefs.SetInt("CONTINUEGAME", 1);
        PlayerPrefs.SetInt("LASTLVLPLAYED", webSockets.restoreLevelId);
        gameExecutionID = webSockets.restoreGameExecutionID;

        if(webSockets.restoreLevelId != 0)
        {
          StartCoroutine(PrepareLevels());
          //Debug.Log("levelsToContinue: " + webSockets.restoreLevelId);
          int levelsToContinue = webSockets.restoreLevelId - 1;
          PlayerPrefs.SetInt("ChapterPlayed", levelsToContinue);
          SceneManager.LoadScene("Travel");
        }     
      }
      else
      {
        requestGameExecutionID = true;
        PlayerPrefs.SetInt("RESTORE", 1);
        Debug.Log("NOVO JOGO!");
        PlayerPrefs.SetInt("RESTORE", 0);
        //Debug.Log("Waiting for execution ID...");
        yield return new WaitUntil(() => gameExecutionDone);
        //gameExecutionID = int.Parse(PlayerPrefs.GetString("GAMEEXECUTIONID"));
        //Debug.Log("Game Execution request completed! ID -> " + PlayerPrefs.GetString("GAMEEXECUTIONID"));
      }
    }
  }

  IEnumerator PreparedGameResult()
  {
    if(repetition == false)
    {
      if(webSockets.statusValue > 0)
      {
        //Debug.Log("ERRORSTATUS" + webSockets.statusValue);
        errorStatus = 0;
      }
      else
      {
        errorStatus = 1;
      }
      if(bonusgameResult == false || (bonusgameResult == true && lastBonusSample == true))
      {
        yield return StartCoroutine(webRequests.PostGameResult(errorStatus.ToString(), "0", currentActionID.ToString(),  gameExecutionID.ToString(), startTime, endTime, currentWord));     
        //Debug.Log("LOG POST GAME RESULT");
        Debug.Log("STATUS: " + errorStatus.ToString() + " ACTIONID: " +  currentActionID.ToString() + " GAMEEXECUTIONID: " +  gameExecutionID.ToString() + " WORD: " + currentWord);
        bonusgameResult = false;
        lastBonusSample = false;
      }
    }
  }
  IEnumerator PlayGuideVoice(string word)
  {
    if(word == "O sapato da menina tem bolas amarelas.")
    {
      yield return StartCoroutine(PlayAudioClip("1"));
    }
    else if(word == "A chuva cai da nuvem.")
    {
      yield return StartCoroutine(PlayAudioClip("3"));
    }
    else if(word == "A mãe faz comida no fogão.")
    {
      yield return StartCoroutine(PlayAudioClip("2"));
    }
    else if(word == "A zebra dorme na sua cama." || word == "A joaninha tira a rolha da garrafa." || word == "O caracol dorme ao sol.")
    {
      yield return StartCoroutine(PlayAudioClip("4"));
    }
  }

  IEnumerator PlayGuideVoiceForReps(int l)
  {
    if(SceneManager.GetActiveScene().name == "Monkey")
    {
      if(l == 1)
      {
        yield return StartCoroutine(PlayAudioClip("rep1"));
      }
      else if (l == 2)
      {
        yield return StartCoroutine(PlayAudioClip("repMacaco"));  
      }     
    }
    else if(SceneManager.GetActiveScene().name == "Chameleon")
    {
      if(l == 1)
      {
        yield return StartCoroutine(PlayAudioClip("rep1"));
      }
      else if (l == 2)
      {
        yield return StartCoroutine(PlayAudioClip("repCamaleao"));
      }
    }
    else if(SceneManager.GetActiveScene().name == "Octopus")
    {
      if(l == 1)
      {
        yield return StartCoroutine(PlayAudioClip("rep1"));
      }
      else if (l == 2)
      {
        yield return StartCoroutine(PlayAudioClip("repPolvo"));
      }
    }
  }

  void RecordSound(int timer)
  {
    userRecording = GetComponent<AudioSource>();
    userRecording.clip = Microphone.Start("", true, timer, 48000);
  }

  void SaveSound(string fileName)
  {
    SavWav.Save(fileName + ".wav", userRecording.clip);
  }

  IEnumerator PlayAudioClip(string clipToPlay)
  {
    foreach (AudioClip clip in clips)
    {
      if(clip.name == clipToPlay)
      {
        aud.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
      }
    }
  }

  public void SendHelp()
  {    
    if(activeHelpButton)
    {
      webSockets.HelpRequest(therapistID);
      Debug.Log("AJUDA");
    }
  }

  IEnumerator PlayHelpVoice()
  {
    yield return StartCoroutine(PlayAudioClip("help"));
    yield return StartCoroutine(PlayWordName(currentWord));
  }

  public void OnApplicationQuit()
  {
    Debug.Log("Stop WS client and logut");
    
    //Enable screen dimming
    Screen.sleepTimeout = SleepTimeout.SystemSetting;

    string payload = "{\"therapist\": " + therapistID + "}";
    webSockets.PrepareMessage("status", payload);
    webSockets.StopClient(payload);

    if (PlayerPrefs.HasKey("RESTORE"))
    {
      PlayerPrefs.DeleteKey("RESTORE");
    }
    
    Application.Quit();       
  }
  
  IEnumerator StopSession()
  {
    Debug.Log("ENTROU NA FUNCAO STOP SESSION");
    finalChapVoice.Play();
    yield return new WaitForSeconds(6.0f);
    OnApplicationQuit();
  }
    

  

}
