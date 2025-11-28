using UnityEngine;

[System.Serializable]
public class KeyPressData
{
    public float x;
    public float y;
    public float time;

    public KeyPressData(Vector2 position, float time)
    {
        x = position.x;
        y = position.y;
        this.time = time;
    }
}

[System.Serializable]
public class JumpEventData
{
    public float posX;
    public float posY;
    public float timestamp;

    public JumpEventData(Vector2 position, float time)
    {
        posX = position.x;
        posY = position.y;
        timestamp = time;
    }
}

[System.Serializable]
public class MirrorUseEvent
{
    public float posX;
    public float posY;
    public float timestamp;

    public MirrorUseEvent(Vector2 position, float time)
    {
        posX = position.x;
        posY = position.y;
        timestamp = time;
    }
}

[System.Serializable]
public class AllyUseEvent
{
    public float posX;
    public float posY;
    public float timestamp;

    public AllyUseEvent(Vector2 position, float time)
    {
        posX = position.x;
        posY = position.y;
        timestamp = time;
    }
}

[System.Serializable]
public class LOSUseEvent
{
    public float posX;
    public float posY;
    public float timestamp;

    public LOSUseEvent(Vector2 position, float time)
    {
        posX = position.x;
        posY = position.y;
        timestamp = time;
    }
}


[System.Serializable]
public class DeathReasonData
{
    public string reason;
    public float posX;
    public float posY;
    public float timestamp;

    public DeathReasonData(string reason, Vector2 position, float time)
    {
        this.reason = reason;
        posX = position.x;
        posY = position.y;
        timestamp = time;
    }
}

[System.Serializable]
public class EnemyKillData
{
    public string reason;
    public float posX;
    public float posY;
    public float timestamp;
    public string enemyType;

    public EnemyKillData(string reason, Vector2 position, float time, string enemyType)
    {
        this.reason = reason;
        posX = position.x;
        posY = position.y;
        timestamp = time;
        this.enemyType = enemyType;
    }
}

[System.Serializable]
public class PortalTraversalData
{
    public string objectType;
    public float fromX;
    public float fromY;
    public float toX;
    public float toY;
    public float timestamp;
    public float velocity;

    public PortalTraversalData(string objectType, Vector2 from, Vector2 to, float velocity, float timestamp)
    {
        this.objectType = objectType;
        this.fromX = from.x;
        this.fromY = from.y;
        this.toX = to.x;
        this.toY = to.y;
        this.timestamp = timestamp;
        this.velocity = velocity;
    }
}
