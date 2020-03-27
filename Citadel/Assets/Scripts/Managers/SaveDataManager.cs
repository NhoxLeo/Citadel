using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveDataManager : MonoBehaviour
{
    public string savePath;
    public string prefPath;
    public List<bool> levelsUnlockStatus;

    public string INPUT_FORWARD = "W";
    public string INPUT_LEFT = "A";
    public string INPUT_BACKWARDS = "S";
    public string INPUT_RIGHT = "D";
    public string INPUT_CROUCH = "C";
    public string INPUT_SPRINT = "LeftShift";
    public string INPUT_JUMP = "Space";
    public string INPUT_USE = "E";
    public string INPUT_RELOAD = "R";
    public string INPUT_SHOOT = "Mouse0";
    public string INPUT_QUICKCYCLE = "Q";
    public string INPUT_MAP = "M";
    public float SENSITIVITY = 120;

    public bool hasReadData = false;

    // Start is called before the first frame update
    private void Start()
    {
        ReadSaveData();
        ReadPrefData();

        hasReadData = true;
    }
    private void Update()
    {
        #region Commands
        if (Input.GetKey(KeyCode.Slash))
        {
            string comboKeyCode = Input.inputString.ToLower();
            if (comboKeyCode == "x")
            {
                WriteDefaultSaveValues();
                ReadSaveData();
                UpdateSaveData();
                ReadSaveData();
            }
            else if (comboKeyCode == "c")
            {
                WriteCompleteValues();
                ReadSaveData();
                UpdateSaveData();
                ReadSaveData();
            }
        }
        #endregion
    }

    public void ReadSaveData()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + savePath))
            {
                //Debug.Log("Reading From: " + Application.persistentDataPath + "/" + savePath);
                StreamReader reader = new StreamReader(Application.persistentDataPath + "/" + savePath);
                int currentIndex = 0;

                while (!reader.EndOfStream)
                {
                    string inp_ln = reader.ReadLine(); //Read In Achievement Data
                    if (inp_ln == "True")
                    {
                        //Debug.Log("Level "+currentIndex+" is Unlocked");
                        levelsUnlockStatus[currentIndex] = true;
                    }
                    else
                    {
                        levelsUnlockStatus[currentIndex] = false;
                    }
                    currentIndex++;
                }
                reader.Close();

                LevelSelectManager levelSelectManager = GameObject.FindObjectOfType<LevelSelectManager>();
                if (levelSelectManager)
                {
                    levelSelectManager.UpdateLevels(levelsUnlockStatus);
                }
            }
            else
            {
                System.IO.File.WriteAllText(Application.persistentDataPath + "/" + savePath, ""); //createfile
                WriteDefaultSaveValues();
                ReadSaveData();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void ReadPrefData()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + prefPath))
            {
                StreamReader reader = new StreamReader(Application.persistentDataPath + "/" + prefPath);

                string inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_FORWARD = inp_ln; //Forward
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_LEFT = inp_ln; //Left
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_BACKWARDS = inp_ln; //Backwards
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_RIGHT = inp_ln; //Right
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_CROUCH = inp_ln; //Crouch
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_SPRINT = inp_ln; //Sprint
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_JUMP = inp_ln; //Jump
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_USE = inp_ln; //Use
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_RELOAD = inp_ln; //Reload
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_SHOOT = inp_ln; //Shoot
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_QUICKCYCLE = inp_ln; //Quick Cycle
                inp_ln = reader.ReadLine(); //Read In Pref Data
                INPUT_MAP = inp_ln; //MAP

                inp_ln = reader.ReadLine(); //Read In Pref Data
                if(SlidersUI.instance)                
                {                  
                    SlidersUI.instance.sfxSlider.value = float.Parse(inp_ln);
                }
                GameVars.instance.sfxVolumeScale = float.Parse(inp_ln); //SFX Volume

                inp_ln = reader.ReadLine(); //Read In Pref Data
                if (SlidersUI.instance)
                {
                    SlidersUI.instance.musicSlider.value = float.Parse(inp_ln);
                }
                GameVars.instance.musicVolumeScale = float.Parse(inp_ln); //Music Volume

                inp_ln = reader.ReadLine(); //Read In Pref Data
                SENSITIVITY = float.Parse(inp_ln); //Sensitivty

                reader.Close();
            }
            else
            {
                System.IO.File.WriteAllText(Application.persistentDataPath + "/" + prefPath, ""); //createfile
                WriteDefaultPrefValues();
                ReadPrefData();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void WriteDefaultSaveValues()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + savePath))
            {
                StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + savePath, false);
                writer.WriteLine(true); //Level1
                writer.WriteLine(false); //Level2
                writer.WriteLine(false); //Level3
                writer.WriteLine(false); //Level4
                writer.WriteLine(false); //Level5
                writer.WriteLine(false); //Level6
                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void WriteDefaultPrefValues()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + prefPath))
            {
                StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + prefPath, false);
                writer.WriteLine(INPUT_FORWARD); //Forward
                writer.WriteLine(INPUT_LEFT); //Left
                writer.WriteLine(INPUT_BACKWARDS); //Backwards
                writer.WriteLine(INPUT_RIGHT); //Right
                writer.WriteLine(INPUT_CROUCH); //Crouch
                writer.WriteLine(INPUT_SPRINT); //Sprint
                writer.WriteLine(INPUT_JUMP); //Jump
                writer.WriteLine(INPUT_USE); //Use
                writer.WriteLine(INPUT_RELOAD); //Reload
                writer.WriteLine(INPUT_SHOOT); //Shoot
                writer.WriteLine(INPUT_QUICKCYCLE); //Quick Cycle
                writer.WriteLine(INPUT_MAP); //Map

                writer.WriteLine(GameVars.instance.sfxVolumeScale); //SFX Volume
                writer.WriteLine(GameVars.instance.musicVolumeScale); //Music Volume

                writer.WriteLine(SENSITIVITY); //Sensitivty

                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void UpdateSaveData()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + savePath))
            {
                StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + savePath, false);
                foreach (bool levelStatus in levelsUnlockStatus)
                {
                    writer.WriteLine(levelStatus);
                }
                writer.Close();
            }
            else
            {
                System.IO.File.WriteAllText(Application.persistentDataPath + "/" + savePath, ""); //createfile
                UpdateSaveData();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void UpdatePrefData()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + prefPath))
            {
                StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + prefPath, false);
                writer.WriteLine(INPUT_FORWARD); //Forward
                writer.WriteLine(INPUT_LEFT); //Left
                writer.WriteLine(INPUT_BACKWARDS); //Backwards
                writer.WriteLine(INPUT_RIGHT); //Right
                writer.WriteLine(INPUT_CROUCH); //Crouch
                writer.WriteLine(INPUT_SPRINT); //Sprint
                writer.WriteLine(INPUT_JUMP); //Jump
                writer.WriteLine(INPUT_USE); //Use
                writer.WriteLine(INPUT_RELOAD); //Reload
                writer.WriteLine(INPUT_SHOOT); //Shoot
                writer.WriteLine(INPUT_QUICKCYCLE); //Quick Cycle
                writer.WriteLine(INPUT_MAP); //Map

                writer.WriteLine(GameVars.instance.sfxVolumeScale); //SFX Volume
                writer.WriteLine(GameVars.instance.musicVolumeScale); //Music Volume

                writer.WriteLine(SENSITIVITY); //Sensitivty

                writer.Close();
            }
            else
            {
                System.IO.File.WriteAllText(Application.persistentDataPath + "/" + prefPath, ""); //createfile
                UpdatePrefData();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void WriteCompleteValues()
    {
        try
        {
            if (File.Exists(Application.persistentDataPath + "/" + savePath))
            {
                StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + savePath, false);
                writer.WriteLine(true); //Level1
                writer.WriteLine(true); //Level2
                writer.WriteLine(true); //Level3
                writer.WriteLine(true); //Level4
                writer.WriteLine(true); //Level5
                writer.WriteLine(true); //Level6
                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
