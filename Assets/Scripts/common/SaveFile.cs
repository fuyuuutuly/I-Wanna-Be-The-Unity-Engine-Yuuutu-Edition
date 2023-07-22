using System;

[Serializable]
internal class SaveFile
{
    public Difficulty difficulty;
    public int death;
    public int time;

    public float playerX;
    public float playerY;
    public Gravity playerGrav;

    public string scene;
}