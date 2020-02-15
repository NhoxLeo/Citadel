using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveDataManager : MonoBehaviour
{
    public string savePath;
    public List<bool> levelsUnlockStatus;

    // Start is called before the first frame update
    private void Start()
    {
        ReadSaveData();
    }

    private void Update()
    {
        #region Commands
        if (Input.GetKey(KeyCode.Slash))
        {
            string comboKeyCode = Input.inputString.ToLower();
            if (comboKeyCode == "x")
            {
                WriteDefaultValues();
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
                WriteDefaultValues();
                ReadSaveData();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void WriteDefaultValues()
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
}
