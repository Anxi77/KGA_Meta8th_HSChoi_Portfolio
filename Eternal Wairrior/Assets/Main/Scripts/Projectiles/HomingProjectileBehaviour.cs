using UnityEngine;

public class HomingProjectileBehavior : StandardProjectileBehavior
{
    protected Enemy targetEnemy;

    public override void UpdateProjectile(BaseProjectile projectile)
    {
        base.UpdateProjectile(projectile);
        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf)
        {
            FindTarget(projectile);
        }
    }

    protected override void Move(BaseProjectile projectile)
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeSelf)
        {
            Vector2 direction = (targetEnemy.transform.position - projectile.transform.position).normalized;
            projectile.transform.up = direction;
            projectile.transform.Translate(direction * projectile.Stats.moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            base.Move(projectile);
        }
    }

    protected virtual void FindTarget(BaseProjectile projectile)
    {
        float nearestDistance = float.MaxValue;
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            float distance = Vector2.Distance(projectile.transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetEnemy = enemy;
            }
        }
    }
}