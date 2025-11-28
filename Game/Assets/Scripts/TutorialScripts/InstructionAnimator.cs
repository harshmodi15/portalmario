using UnityEngine;

public class InstructionAnimator : MonoBehaviour
{
    public Sprite[] sprites; // Array to hold the PNG images (Sprites)
    public float switchTime = 0.5f; // Time between sprite switches

    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        InvokeRepeating("ChangeSprite", 0f, switchTime); // Start changing sprites
    }

    void ChangeSprite()
    {
        if (sprites.Length == 0) return; // Avoid errors if no sprites are assigned

        // Set the next sprite
        spriteRenderer.sprite = sprites[currentSpriteIndex];

        // Update the index, looping back to 0 if we reach the end
        currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
    }
}