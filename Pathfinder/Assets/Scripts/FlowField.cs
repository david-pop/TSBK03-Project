using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField {

    private int[,] integratorField;
    private int width, height;
    private int[,] obstacleField;
    private int cellSize;

    public FlowField(int[,] obstacleField, int cellSize, int goalX, int goalZ){
        width = obstacleField.GetLength(0);
        height = obstacleField.GetLength(1);

        integratorField = new int[width, height];

        for (int x = 0; x < width; x++){
            for (int z = 0; z < height; z++){
                integratorField[x, z] = int.MaxValue;
            }
        }

        this.obstacleField = obstacleField;

        integrate(goalX, goalZ, 0);
    }

    private void integrate(int x, int z, int cost){
        if(x >  width - 1 || x < 0 ||
           z > height - 1 || z < 0){
            return;
        }

        if(obstacleField[x, z] == 1){
            return;
        }

        if(cost > integratorField[x,z]){
            integratorField[x, z] = cost;

            integrate(x + 1, z, cost + 1);
            integrate(x - 1, z, cost + 1);
            integrate(x, z + 1, cost + 1);
            integrate(x, z - 1, cost + 1);

            integrate(x + 1, z + 1, cost + 1);
            integrate(x - 1, z + 1, cost + 1);
            integrate(x - 1, z - 1, cost + 1);
            integrate(x + 1, z - 1, cost + 1);
        }
        else{
            return;
        }
    }

    public Vector2 getDirection(int x, int z){
        if (x > width - 1 || x < 0 ||
           z > height - 1 || z < 0){
            return Vector2.zero;
        }else{
            int left = ((x - 1) < 0 || obstacleField[x - 1, z] == 1) ? 
                integratorField[x, z] : integratorField[x - 1, z];

            int right = ((x + 1) > width || obstacleField[x + 1, z] == 1) ?
                integratorField[x, z] : integratorField[x + 1, z];

            int bottom = ((z - 1) < 0 || obstacleField[x, z - 1] == 1) ?
                integratorField[x, z] : integratorField[x, z - 1];

            int top = ((z + 1) > height || obstacleField[x, z + 1] == 1) ?
                integratorField[x, z] : integratorField[x, z + 1];

            return new Vector2(left - right, bottom - top);
        }
    }

    public Vector2 getDirection(float x, float z){
        return getDirection(
            Mathf.FloorToInt(x / cellSize), Mathf.FloorToInt(z / cellSize));
    }
}
