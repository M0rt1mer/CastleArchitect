using UnityEngine;


public static class SquareDiamondNoise {

    #region old method

    public static void DiamondStep( float[,] heightmap, BoundsInt rectangle, float amplitude, int numSteps ) {

        if(numSteps == 0) { //interpolate
            /*if( rectangle.xMin > 8 || rectangle.yMin > 8 )
                return;*/
            float valMinXminY = heightmap[rectangle.xMin,rectangle.yMin];
            float valMinXmaxY = heightmap[rectangle.xMin,rectangle.yMax];
            float valMaxXminY = heightmap[rectangle.xMax,rectangle.yMin];
            float valMaxXmaxY = heightmap[rectangle.xMax,rectangle.yMax];

            float width = rectangle.size.x;
            float height = rectangle.size.y;

            for(int x = rectangle.xMin; x <= rectangle.xMax; x++) {
                for(int y = rectangle.yMin; y <= rectangle.yMax; y++) {
                    float xParam = (x - rectangle.xMin) / width;
                    float yParam = (y - rectangle.yMin) / height;
                    heightmap[x,y] = valMinXminY * (1- xParam) * (1-yParam) +
                                      valMaxXminY * xParam * (1-yParam) +
                                      valMinXmaxY * (1-xParam) * yParam +
                                      valMaxXmaxY * xParam * yParam;
                }
            }

        } else { //noise

            Vector2Int midPoint = new Vector2Int( Mathf.RoundToInt( rectangle.center.x ), Mathf.RoundToInt( rectangle.center.y ) );
            heightmap[midPoint.x,midPoint.y] = (heightmap[rectangle.xMin,rectangle.yMin] +
                                                 heightmap[rectangle.xMin, rectangle.yMax] +
                                                 heightmap[rectangle.xMax, rectangle.yMin] +
                                                 heightmap[rectangle.xMax, rectangle.yMax]) / 4 +
                                                Random.value * amplitude;

            heightmap[rectangle.xMin,midPoint.y] = (heightmap[rectangle.xMin, rectangle.yMin] + heightmap[rectangle.xMin, rectangle.yMax]) / 2 + Random.value * amplitude;
            heightmap[rectangle.xMax,midPoint.y] = (heightmap[rectangle.xMax, rectangle.yMin] + heightmap[rectangle.xMax, rectangle.yMax]) / 2 + Random.value * amplitude;

            heightmap[midPoint.x,rectangle.yMin] = (heightmap[rectangle.xMin, rectangle.yMin] + heightmap[rectangle.xMax, rectangle.yMin]) / 2 + Random.value * amplitude;
            heightmap[midPoint.x,rectangle.yMax] = (heightmap[rectangle.xMin, rectangle.yMax] + heightmap[rectangle.xMax, rectangle.yMax]) / 2 + Random.value * amplitude;

            DiamondStep( heightmap, CreateNewRect( rectangle.xMin, rectangle.yMin,  midPoint.x, midPoint.y ), amplitude / 2, numSteps - 1 );
            DiamondStep( heightmap, CreateNewRect( rectangle.xMin, midPoint.y,  midPoint.x, rectangle.yMax ), amplitude / 2, numSteps - 1 );
            DiamondStep( heightmap, CreateNewRect( midPoint.x, rectangle.yMin,  rectangle.xMax, midPoint.y ), amplitude / 2, numSteps - 1 );
            DiamondStep( heightmap, CreateNewRect( midPoint.x, midPoint.y,  rectangle.xMax, rectangle.yMax ), amplitude / 2, numSteps - 1 );

        }
    }

    private static BoundsInt CreateNewRect( int xMin, int yMin, int xMax, int yMax ) {
        return new BoundsInt( xMin, yMin, 0, xMax - xMin, yMax - yMin, 0 );
    }


    public static void FirstStep( float[,] heightmap, int numSteps ) {

        int width = heightmap.GetLength( 0 );
        int height = heightmap.GetLength( 1 );

        heightmap[0, 0] = Random.value * 0.5f;
        heightmap[0, height - 1] = Random.value * 0.5f;
        heightmap[width - 1, 0] = Random.value * 0.5f;
        heightmap[width - 1, height - 1] = Random.value * 0.5f;

        DiamondStep( heightmap, new BoundsInt( 0, 0, 0, width - 1, height - 1, 0 ), 0.25f, numSteps );

    }

    #endregion

    /// <summary>
    /// Performs a step of diamond noise algorithm
    /// </summary>
    /// <param name="heightmap">Map of heights, expected to be 2^max_steps + 1</param>
    /// <param name="amplitude">amplitude to apply in this step</param>
    /// <param name="step">which step is it</param>
    /// <returns></returns>
    private static void BetterDiamondStep( float[,] heightmap, float amplitude, int step ) {

        // middle point step
        int numMidPointsInLine = 1 << step;
        if( numMidPointsInLine >= heightmap.GetLength( 0 )) {
            return;
        }

        int offsetStep = (heightmap.GetLength( 0 )-1) >> step;
        int halfStep = offsetStep / 2;
        for(int x = 0; x < numMidPointsInLine; x++) {
            for(int y = 0; y < numMidPointsInLine; y++) {
                //midpointMap[x, y] = ( cornermap[x, y] + cornermap[x + 1, y] + cornermap[x, y + 1] + cornermap[x + 1, y + 1] )/4 + Random.value * amplitude;
                heightmap[halfStep + x * offsetStep, halfStep + y * offsetStep] = (heightmap[x * offsetStep, y * offsetStep] +
                                                                                heightmap[(x + 1) * offsetStep, y * offsetStep] +
                                                                                heightmap[x * offsetStep, (y + 1) * offsetStep] +
                                                                                heightmap[(x + 1) * offsetStep, (y + 1) * offsetStep]) / 4 + (Random.value * amplitude * 2) - amplitude;
            }
        }
        
        //corner step odd
        for(int x = 0; x < numMidPointsInLine; x++) {
            for(int y = 0; y < numMidPointsInLine + 1; y++) {
                if(y == 0) {
                    heightmap[halfStep + x * offsetStep, y * offsetStep] = (heightmap[x * offsetStep, y * offsetStep] +
                                                                                heightmap[(x + 1) * offsetStep, y * offsetStep] +
                                                                                heightmap[x * offsetStep + halfStep, y * offsetStep + halfStep]) / 3 + (Random.value * amplitude * 2) - amplitude;
                } else if(y == numMidPointsInLine) {
                    heightmap[halfStep + x * offsetStep, y * offsetStep] = (heightmap[x * offsetStep, y * offsetStep] +
                                                                                heightmap[(x + 1) * offsetStep, y * offsetStep] +
                                                                                heightmap[x * offsetStep + halfStep, y * offsetStep - halfStep]) / 3 + (Random.value * amplitude * 2) - amplitude;
                } else heightmap[halfStep + x * offsetStep, y * offsetStep] = (heightmap[halfStep + x * offsetStep, y * offsetStep + halfStep] +
                                                                                  heightmap[halfStep + x * offsetStep, y * offsetStep - halfStep] +
                                                                                  heightmap[(x + 1) * offsetStep, y * offsetStep] +
                                                                                  heightmap[x * offsetStep, y * offsetStep]) / 4 + (Random.value * amplitude * 2) - amplitude;
            }
        }
        //corner step even
        for(int x = 0; x < numMidPointsInLine + 1; x++) {
            for(int y = 0; y < numMidPointsInLine; y++) {
                if(x == 0) {
                    heightmap[x * offsetStep, halfStep + y * offsetStep] = (heightmap[x * offsetStep, y * offsetStep] +
                                                                                heightmap[x * offsetStep, (y + 1) * offsetStep] +
                                                                                heightmap[x * offsetStep + halfStep, y * offsetStep + halfStep]) / 3 + (Random.value * amplitude * 2) - amplitude;
                } else if(x == numMidPointsInLine) {
                    heightmap[x * offsetStep, halfStep + y * offsetStep] = (heightmap[x * offsetStep, y * offsetStep] +
                                                                                heightmap[x * offsetStep, (y + 1) * offsetStep] +
                                                                                heightmap[x * offsetStep - halfStep, y * offsetStep + halfStep]) / 3 + (Random.value * amplitude * 2) - amplitude;
                } else heightmap[x * offsetStep, halfStep + y * offsetStep] = (heightmap[x * offsetStep + halfStep, halfStep + y * offsetStep] +
                                                                                heightmap[x * offsetStep - halfStep, halfStep + y * offsetStep] +
                                                                                heightmap[x * offsetStep, (y + 1) * offsetStep] +
                                                                                heightmap[x * offsetStep, y * offsetStep]) / 4 + (Random.value * amplitude * 2) - amplitude;
            }
        }

        //BetterDiamondStep( heightmap, amplitude/2, step+1 );

    }

    public static void InitializeBetterSquareDiamondNoise( int numSteps ) {
        smallHeightmap = InitHeightmap( numSteps );
        amplitude = 0.25f;
        currentStep = 0;
    }

    private static int currentStep;
    private static float amplitude;

    public static void BetterSquareDiamondSingleStep() {
        BetterDiamondStep( smallHeightmap, amplitude, currentStep );
        amplitude /= 2;
        currentStep++;
    }

    public static float[,] BetterSquareDiamondNoiseFinalize( int finalResolution ) {
        return UpscaleMap( smallHeightmap, finalResolution );
    }

    public static float[,] BetterSquareDiamondNoise( int finalResolution ) {

        int numSteps = Mathf.FloorToInt( Mathf.Log(finalResolution,2) );

        float[,] heightmap = InitHeightmap( numSteps );

        for(int i = 0; i<numSteps;i++)
            BetterDiamondStep( heightmap, 0.33f/(1<<i), i );

        return heightmap;
        //return UpscaleMap( heightmap, finalResolution );
        
    }

    private static float[,] InitHeightmap( int numSteps ) {
        int heightMapResolution = 1 << numSteps;
        float[,] heightmap = new float[heightMapResolution + 1, heightMapResolution + 1];
        heightmap[0, 0] = Random.value * 0.66f;
        heightmap[0, heightMapResolution] = Random.value * 0.66f;
        heightmap[heightMapResolution, 0] = Random.value * 0.66f;
        heightmap[heightMapResolution, heightMapResolution] = Random.value * 0.66f;
        return heightmap;
    }

    private static float[,] UpscaleMap( float[,] heightmap, int finalResolution ) {
        int ratio = (finalResolution-1) / (heightmap.GetLength( 0 )-1);
        if(ratio < 2)
            return heightmap;
        float[,] fullheightMap = new float[finalResolution, finalResolution];
        for(int x = 0; x < (heightmap.GetLength( 0 ) - 1); x++) {
            for(int y = 0; y < (heightmap.GetLength( 0 ) - 1); y++) {

                for(int x1 = 0; x1 < ratio; x1++)
                    for(int y1 = 0; y1 < ratio; y1++) {
                        float xParam = x1 / (float)ratio;
                        float yParam = y1 / (float)ratio;

                        fullheightMap[x * ratio + x1, y * ratio + y1] = heightmap[x, y] * (1 - xParam) * (1 - yParam) +
                                                                        heightmap[x + 1, y] * xParam * (1 - yParam) +
                                                                        heightmap[x, y + 1] * (1 - xParam) * yParam +
                                                                        heightmap[x + 1, y + 1] * xParam * yParam;
                        //fullheightMap[x * ratio + x1, y * ratio + y1] = heightmap[x, y];

                    }


            }
        }
        smallHeightmap = heightmap;
        return fullheightMap;
    }

    public static float[,] smallHeightmap;

}

