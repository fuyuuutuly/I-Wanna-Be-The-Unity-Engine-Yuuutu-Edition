using System;

[Serializable]
class SaveFile
{
    public Difficulty difficulty;
    public int death;
    public int time;

    public float playerX;
    public float playerY;
    public int playerGrav;

    public string scene;
}
