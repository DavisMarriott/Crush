using UnityEngine;

public class SoundManagerFoley : MonoBehaviour
{
    private AudioSource audioSource;
    // FOOTSTEPS
    public AudioClip footstep01;
    public AudioClip footstep02;
    public AudioClip footstep03;
    public AudioClip footstep04;
    public AudioClip footstep05;
    public AudioClip footstep06;
    // CLOTHING
    public AudioClip clothing01;
    public AudioClip clothing02;
    public AudioClip clothing03;
    public AudioClip clothing04;
    public AudioClip clothing05;
    public AudioClip clothing06;
    public AudioClip clothing07;
    public AudioClip clothing08;
    public AudioClip clothing09;
    public AudioClip clothing10;
    // IMPACTS
    public AudioClip impact01;
    public AudioClip impact02;
    public AudioClip impact03;
    public AudioClip impact04;
    public AudioClip impact05;
    public AudioClip impact06;
    // VOICE
    public AudioClip voiceNeutral01;
    public AudioClip voiceNeutral02;
    public AudioClip voiceNeutral03;
    public AudioClip voiceNeutral04;
    public AudioClip voiceNeutral05;
    public AudioClip voicePositive01;
    public AudioClip voicePositive02;
    public AudioClip voicePositive03;
    public AudioClip voicePositive04;
    public AudioClip voicePositive05;
    public AudioClip voiceNegative01;
    public AudioClip voiceNegative02;
    public AudioClip voiceNegative03;
    public AudioClip voiceNegative04;
    public AudioClip voiceNegative05;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing from GameObject!");
        }
    }

    // FOOTSTEPS
    
    public void Footstep01()
    {
        audioSource.PlayOneShot(footstep01);
    }
    
    public void Footstep02()
    {
        audioSource.PlayOneShot(footstep02);
    }
    
    public void Footstep03()
    {
        audioSource.PlayOneShot(footstep03);
    }
    
    public void Footstep04()
    {
        audioSource.PlayOneShot(footstep04);
    }
    
    public void Footstep05()
    {
        audioSource.PlayOneShot(footstep05);
    }
    
    public void Footstep06()
    {
        audioSource.PlayOneShot(footstep06);
    }

    // CLOTHING
    public void Clothing01()
    {
        audioSource.PlayOneShot(clothing01);
    }
    
    public void Clothing02()
    {
        audioSource.PlayOneShot(clothing02);
    }
    
    public void Clothing03()
    {
        audioSource.PlayOneShot(clothing03);
    }
    
    public void Clothing04()
    {
        audioSource.PlayOneShot(clothing04);
    }
    
    public void Clothing05()
    {
        audioSource.PlayOneShot(clothing05);
    }
    
    public void Clothing06()
    {
        audioSource.PlayOneShot(clothing06);
    }
    
    public void Clothing07()
    {
        audioSource.PlayOneShot(clothing07);
    }
    
    public void Clothing08()
    {
        audioSource.PlayOneShot(clothing08);
    }
    
    public void Clothing09()
    {
        audioSource.PlayOneShot(clothing09);
    }
    
    public void Clothing10()
    {
        audioSource.PlayOneShot(clothing10);
    }
    
    // IMPACTS
    
    public void Impact01()
    {
        audioSource.PlayOneShot(impact01);
    }
    
    public void Impact02()
    {
        audioSource.PlayOneShot(impact02);
    }
    
    public void Impact03()
    {
        audioSource.PlayOneShot(impact03);
    }
    
    public void Impact04()
    {
        audioSource.PlayOneShot(impact04);
    }
    
    public void Impact05()
    {
        audioSource.PlayOneShot(impact05);
    }
    
    public void Impact06()
    {
        audioSource.PlayOneShot(impact06);
    }
    
}
