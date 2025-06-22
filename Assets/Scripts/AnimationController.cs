using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    private readonly Animator _animator;
    private readonly Dictionary<string, AnimationClip> _clipCache;
    private readonly Dictionary<string, int> _parameterHashCache;
    
    // Константы для общих параметров
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _speedMultiplierHash = Animator.StringToHash("SpeedMultiplier");
    
    public AnimationController(Animator animator)
    {
        _animator = animator;
        _clipCache = new Dictionary<string, AnimationClip>();
        _parameterHashCache = new Dictionary<string, int>();
        
        // Кэшируем все клипы при инициализации
        CacheAnimationClips();
    }
    
    private void CacheAnimationClips()
    {
        if (_animator.runtimeAnimatorController == null) return;
        
        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (!_clipCache.ContainsKey(clip.name))
            {
                _clipCache[clip.name] = clip;
            }
        }
    }
    
    private int GetParameterHash(string parameterName)
    {
        if (!_parameterHashCache.ContainsKey(parameterName))
        {
            _parameterHashCache[parameterName] = Animator.StringToHash(parameterName);
        }
        return _parameterHashCache[parameterName];
    }
    
    /// <summary>
    /// Воспроизводит анимацию с заданной скоростью на основе желаемой длительности
    /// </summary>
    /// <param name="animationName">Имя анимации</param>
    /// <param name="desiredDuration">Желаемая длительность анимации</param>
    /// <param name="speedParameterName">Имя параметра скорости в аниматоре</param>
    /// <param name="layer">Слой анимации</param>
    /// <param name="normalizedTime">Нормализованное время начала</param>
    public void PlayAnimationWithDuration(string animationName, float desiredDuration, 
        string speedParameterName = "SpeedMultiplier", int layer = 0, float normalizedTime = 0f)
    {
        if (!_clipCache.TryGetValue(animationName, out var clip))
        {
            Debug.LogWarning($"Animation clip '{animationName}' not found!");
            return;
        }
        
        float speedMultiplier = clip.length / desiredDuration;
        int paramHash = GetParameterHash(speedParameterName);
        
        _animator.SetFloat(paramHash, speedMultiplier);
        _animator.Play(animationName, layer, normalizedTime);
    }
    
    /// <summary>
    /// Воспроизводит анимацию с обычной скоростью
    /// </summary>
    public void PlayAnimation(string animationName, int layer = 0, float normalizedTime = 0f)
    {
        _animator.Play(animationName, layer, normalizedTime);
    }
    
    /// <summary>
    /// Синхронизирует скорость анимации с физической скоростью движения
    /// </summary>
    /// <param name="currentSpeed">Текущая скорость движения</param>
    /// <param name="baseSpeed">Базовая скорость для нормализации</param>
    /// <param name="speedParameterName">Имя параметра скорости</param>
    public void SyncAnimationWithMovement(float currentSpeed, float baseSpeed, 
        string speedParameterName = "Speed")
    {
        float normalizedSpeed = currentSpeed / baseSpeed;
        int paramHash = GetParameterHash(speedParameterName);
        _animator.SetFloat(paramHash, normalizedSpeed);
    }
    
    /// <summary>
    /// Устанавливает параметр float в аниматоре
    /// </summary>
    public void SetFloat(string parameterName, float value)
    {
        int paramHash = GetParameterHash(parameterName);
        _animator.SetFloat(paramHash, value);
    }
    
    /// <summary>
    /// Устанавливает параметр bool в аниматоре
    /// </summary>
    public void SetBool(string parameterName, bool value)
    {
        int paramHash = GetParameterHash(parameterName);
        _animator.SetBool(paramHash, value);
    }
    
    /// <summary>
    /// Устанавливает параметр trigger в аниматоре
    /// </summary>
    public void SetTrigger(string parameterName)
    {
        int paramHash = GetParameterHash(parameterName);
        _animator.SetTrigger(paramHash);
    }
    
    /// <summary>
    /// Сбрасывает параметр скорости к базовому значению
    /// </summary>
    public void ResetSpeedParameter(string speedParameterName = "SpeedMultiplier", float baseValue = 1f)
    {
        SetFloat(speedParameterName, baseValue);
    }
    
    /// <summary>
    /// Получает информацию о текущем состоянии анимации
    /// </summary>
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer = 0)
    {
        return _animator.GetCurrentAnimatorStateInfo(layer);
    }
    
    /// <summary>
    /// Проверяет, завершена ли анимация
    /// </summary>
    public bool IsAnimationComplete(int layer = 0, float threshold = 0.95f)
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime >= threshold;
    }
    
    /// <summary>
    /// Получает длительность анимационного клипа
    /// </summary>
    public float GetClipLength(string animationName)
    {
        if (_clipCache.TryGetValue(animationName, out var clip))
        {
            return clip.length;
        }
        
        Debug.LogWarning($"Animation clip '{animationName}' not found!");
        return 0f;
    }
    
    /// <summary>
    /// Проверяет, есть ли анимационный клип с заданным именем
    /// </summary>
    public bool HasClip(string animationName)
    {
        return _clipCache.ContainsKey(animationName);
    }
}