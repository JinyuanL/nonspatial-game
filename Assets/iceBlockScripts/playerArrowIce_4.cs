﻿using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class playerArrowIce_4 : MonoBehaviour {
	private const int NUM_ROWS = 6;
	private const int NUM_COLS = 6;
	public iceBlock_4[] ices;

	private Vector3 direction;
	private Vector3 prevMoveDir;

	private Vector3 right;
	private Vector3 left;
	private Vector3 up;
	private Vector3 down;
	private Vector2 square;
	private Vector2 predictedSquare;
	private bool victorious;

	//UI for playing a new game
	private Button yes;
	private Button no;
	private Image victoryPanel;
	private Text victoryText;

	/* DATA COLLECTION */
	private int plays;
	private int victories;
	private int resets;

	/* MOVEMENT DATA */
	private int actions; //number of times player either moves or pushes
	private int moves; 	//number of times player moves from one square to another
	private int pushes; 	//number of times player attempts to push an ice block
	private int successfulPushes; // number of times player pushes ice block and it moves
	private int turns;	//number of times player changes direction
	private int pathTurns; //number of times player's path actually turns (pathTurns <= turns)
	private int longest_straight_path;
	private float avg_turns_per_move;
	private float avg_turns_per_action;
	private float avg_turns_per_push;
	private float avg_path_turns_per_move;

	/* ICE BLOCK "ERROR" DATA */
	private int iceCantMove; 	// number of times player tries to push ice block but something is in the way
	private int iceBlockedByIce; // ice block doesn't move due to presence of other ice block
	private int iceBlockedByOffscreen; // ice block doesn't move due to it being on edge of screen
	private int iceStoppedByIce; // ice block slides and then hits another ice block
	private int iceStoppedByOffscreen; // ice block slides and then hits the edge of the area

	/* TIME DATA */
	private float startTime;
	private float prevActionEndTime;
	private float prevPushEndTime;
	private float prevMoveEndTime;
	private float avg_time_per_action;
	private float avg_time_per_push;
	private float avg_time_per_move;
	private float game_time;
	private float session_time; // time spent in all games combined
	private float sessionStart_time;

	/* PLAYER LOCATION DATA */
	private int left_squares_player;
	private int right_squares_player;
	private int top_squares_player;
	private int bottom_squares_player;
	private int num_repeated_squares_player; // number of times player steps on square they've already been to
	private int num_traversed_squares_player; // total displacement of player
	private int squares_explored_player; // squares player has moved onto
	private float left_right_symmetry_player;
	private float top_bottom_symmetry_player;
	private IList<string> squares_explored_player_list;

	/* ICE LOCATION DATA */
	private int left_squares_ice; //including repetitions
	private int right_squares_ice; //including repetitions
	private int top_squares_ice; //including repetitions
	private int bottom_squares_ice; //including repetitions
	private int num_repeated_squares_ice;
	private int num_traversed_squares_ice;
	private int squares_explored_ice; // squares ice blocks have moved onto/across
	private float left_right_symmetry_ice;
	private float top_bottom_symmetry_ice;
	private IList<string> squares_explored_ice_list;

	/* Keep track of which squares are in which area of board */
	private IList<string> left_squares_list;
	private IList<string> right_squares_list;
	private IList<string> top_squares_list;
	private IList<string> bottom_squares_list;

	private string pathTrace;

	//write to database
	string resultStr;

	void Awake() {
		victorious = false;
		startTime = Time.time;
		sessionStart_time = startTime;
		prevActionEndTime = startTime;
		prevMoveEndTime = startTime;
		prevPushEndTime = startTime;
	}

	// Use this for initialization
	void Start () {
		resultStr = "NEW_GAME,ice,";
		direction = Vector3.right;
		prevMoveDir = direction;
		right = new Vector3(0,0,270);
		left = new Vector3(0,0,90);
		up = new Vector3(0,0,0);
		down = new Vector3(0,0,180);
		square = new Vector2(6,2);
		predictedSquare = new Vector2(6,3); //player faces right to start

		//Data collection variables
		plays = 1;
		victories = 0;
		resets = 0;
		actions = 0;
		moves = 0;
		turns = 0;
		pathTurns = 0;
		longest_straight_path = 0;
		pushes = 0;
		successfulPushes = 0;
		avg_time_per_move = -1f;
		avg_turns_per_move = -1f;
		avg_turns_per_action = -1f;
		avg_turns_per_push = -1f;
		avg_path_turns_per_move = -1f;

		iceCantMove = 0; 	
		iceBlockedByIce = 0; 
		iceBlockedByOffscreen = 0; 
		iceStoppedByIce = 0; 
		iceStoppedByOffscreen = 0; 

		squares_explored_player_list = new List<string>();
		squares_explored_player_list.Add ("62");

		squares_explored_ice_list = new List<string>();
		squares_explored_ice_list.Add ("22");
		squares_explored_ice_list.Add ("52");
		squares_explored_ice_list.Add ("55");

		left_squares_player = 1;
		right_squares_player = 0;
		top_squares_player = 0;
		bottom_squares_player = 1;
		squares_explored_player = 1;
		num_repeated_squares_player = 0;
		num_traversed_squares_player = 1;

		left_squares_ice = 0; // because it will be calculated at end
		right_squares_ice = 0; // because it will be calculated at end
		top_squares_ice = 0; // because it will be calculated at end
		bottom_squares_ice = 0; // because it will be calculated at end
		squares_explored_ice = 0; // because it will be calculated at end
		num_repeated_squares_ice = 0; // because it will be calculated at end
		num_traversed_squares_ice = 0; // because it will be calculated at end

		pathTrace = coordinatesToSquare(square); //starting square

		left_squares_list = new List<string>();
		left_squares_list.Add ("11");
		left_squares_list.Add ("12");
		left_squares_list.Add ("13");
		left_squares_list.Add ("21");
		left_squares_list.Add ("22");
		left_squares_list.Add ("23");
		left_squares_list.Add ("31");
		left_squares_list.Add ("32");
		left_squares_list.Add ("33");
		left_squares_list.Add ("41");
		left_squares_list.Add ("42");
		left_squares_list.Add ("43");
		left_squares_list.Add ("51");
		left_squares_list.Add ("52");
		left_squares_list.Add ("53");
		left_squares_list.Add ("61");
		left_squares_list.Add ("62");
		left_squares_list.Add ("63");

		right_squares_list = new List<string>();
		right_squares_list.Add ("14");
		right_squares_list.Add ("15");
		right_squares_list.Add ("16");
		right_squares_list.Add ("24");
		right_squares_list.Add ("25");
		right_squares_list.Add ("26");
		right_squares_list.Add ("34");
		right_squares_list.Add ("35");
		right_squares_list.Add ("36");
		right_squares_list.Add ("44");
		right_squares_list.Add ("45");
		right_squares_list.Add ("46");
		right_squares_list.Add ("54");
		right_squares_list.Add ("55");
		right_squares_list.Add ("56");
		right_squares_list.Add ("64");
		right_squares_list.Add ("65");
		right_squares_list.Add ("66");

		bottom_squares_list = new List<string>();
		bottom_squares_list.Add ("51");
		bottom_squares_list.Add ("52");
		bottom_squares_list.Add ("53");
		bottom_squares_list.Add ("54");
		bottom_squares_list.Add ("55");
		bottom_squares_list.Add ("56");
		bottom_squares_list.Add ("61");
		bottom_squares_list.Add ("62");
		bottom_squares_list.Add ("63");
		bottom_squares_list.Add ("64");
		bottom_squares_list.Add ("65");
		bottom_squares_list.Add ("66");

		top_squares_list = new List<string>();
		top_squares_list.Add ("11");
		top_squares_list.Add ("12");
		top_squares_list.Add ("13");
		top_squares_list.Add ("14");
		top_squares_list.Add ("15");
		top_squares_list.Add ("16");
		top_squares_list.Add ("21");
		top_squares_list.Add ("22");
		top_squares_list.Add ("23");
		top_squares_list.Add ("24");
		top_squares_list.Add ("25");
		top_squares_list.Add ("26");

		left_right_symmetry_player = -1f;
		top_bottom_symmetry_player = -1f;
		left_right_symmetry_ice = -1f;
		top_bottom_symmetry_ice = -1f;
	
		//Victory UI variables
		yes = GameObject.Find ("Yes").GetComponent<Button>();
		no = GameObject.Find ("No").GetComponent<Button>();
		victoryPanel = GameObject.Find ("Victory").GetComponent<Image>();
		victoryText = GameObject.Find ("Congratulations").GetComponent<Text>();

	}

	public IList<string> getSquaresPlayerExplored() {
		return squares_explored_player_list;
	}

	public int getNumSquaresPlayerExplored() {
		return squares_explored_player_list.Count;
	}

	public int getNumMoves() {
		return moves;
	}

	public Vector3 getDirection() {
		return direction;
	}

	// calculates the longest subsequence of this attempt's path trace without a turn
	// and the avg straight path length for the entire path
	private int getLongestStraightPath() {
		string[] path = pathTrace.Split('-');
		int curCol, curRow, nextCol, nextRow;
		int currentColLength = 0;
		int currentRowLength = 0;
		int longestCol = 0;
		int longestRow = 0;
		for(int i = 0; i < path.Length - 1; i++) {
			if (path[i].Length > 2) { // 3 digit column number
				curCol = Convert.ToInt32(path[i].Substring(0,2));
				curRow = Convert.ToInt32(path[i].Substring(2,1));
			} else { // 2 digit column number (normal)
				curCol = Convert.ToInt32(path[i].Substring(0,1));
				curRow = Convert.ToInt32(path[i].Substring(1,1));
			}
			if (path[i+1].Length > 2) { // 3 digit column number
				nextCol = Convert.ToInt32(path[i+1].Substring(0,2));
				nextRow = Convert.ToInt32(path[i+1].Substring(2,1));
			} else { // 2 digit column number (normal)
				nextCol = Convert.ToInt32(path[i+1].Substring(0,1));
				nextRow = Convert.ToInt32(path[i+1].Substring(1,1));
			}

			// update longest column so far
			if(nextCol == curCol) {
				currentColLength++;
				if(currentColLength > longestCol) {
					longestCol = currentColLength;
				}
			} else {
				currentColLength = 1;
			}

			// update longest row so far
			if(nextRow == curRow) {
				currentRowLength++;
				if(currentRowLength > longestRow) {
					longestRow = currentRowLength;
				}
			} else {
				currentRowLength = 1;
			}
		}

		// set the longest path to the larger of the longest col and row
		if(longestCol > longestRow) {
			return longestCol;
		} else {
			return longestRow;
		}
	}

	private void displayOptions() {
		victoryPanel.enabled = true;
		victoryText.enabled = true;

		yes.GetComponent<Image>().enabled = true;
		yes.interactable = true;
		yes.transform.Find("YesText").GetComponent<Text>().enabled = true;

		no.GetComponent<Image>().enabled = true;
		no.interactable = true;
		no.transform.Find("NoText").GetComponent<Text>().enabled = true;
	}

	private void unDisplayOptions() {
		victoryPanel.enabled = false;
		victoryText.enabled = false;

		yes.GetComponent<Image>().enabled = false;
		yes.interactable = false;
		yes.transform.Find("YesText").GetComponent<Text>().enabled = false;

		no.GetComponent<Image>().enabled = false;
		no.interactable = false;
		no.transform.Find("NoText").GetComponent<Text>().enabled = false;

	}

	private string coordinatesToSquare(Vector2 coordinates) {
		return coordinates.x.ToString () + coordinates.y.ToString ();
	}

	private bool approximately(Vector3 first, Vector3 second) {
		if(Mathf.Approximately(first.x, second.x) && Mathf.Approximately(first.y, second.y) && Mathf.Approximately(first.z, second.z)) {
			return true;
		}
		return false;
	}

	private bool offScreen() {
		if(GameObject.Find ("Block" + coordinatesToSquare(predictedSquare)) == null) {
			// can't move - offscreen
			return true;
		}
		return false;
	}

	// index 0 is 1 if player blocked by offscreen
	// index 1 is 1 if player blocked by ice 
	public bool[] getErrorType() {
		bool[] errors = new bool[2];
		for(int i = 0; i < 2; i++) {
			errors[i] = false;
		}
		if(offScreen ()) {
			errors[0] = true;

		} else if (blockedByIce()) {
			errors[1] = true;
		} 
		return errors;
	}

	public string move() {
		transform.Translate(direction * 2f, Space.World);
		num_traversed_squares_player++;
		string predictedSquareName = coordinatesToSquare(predictedSquare);

		pathTrace += "-" + predictedSquareName;

		Vector3 oldSquare = square; 
		square = predictedSquare; 
		if(!squares_explored_player_list.Contains(predictedSquareName)) {
			squares_explored_player_list.Add(predictedSquareName);
		} else {
			num_repeated_squares_player++;
		}
		predictedSquare.x = 2f * square.x - oldSquare.x; 
		predictedSquare.y = 2f * square.y - oldSquare.y; 
		return predictedSquareName;

	}

	public void turnDown(){
		direction = Vector3.down;
		transform.rotation = Quaternion.Euler(down);
		predictedSquare.x = square.x + 1;
		predictedSquare.y = square.y;

	}

	public void turnUp(){
		direction = Vector3.up;
		transform.rotation = Quaternion.Euler(up);
		predictedSquare.x = square.x - 1;
		predictedSquare.y = square.y;

	}

	public void turnLeft(){
		direction = Vector3.left;
		transform.rotation = Quaternion.Euler(left);
		predictedSquare.x = square.x;
		predictedSquare.y = square.y - 1;

	}

	public void turnRight(){
		direction = Vector3.right;
		transform.rotation = Quaternion.Euler(right);
		predictedSquare.x = square.x;
		predictedSquare.y = square.y + 1;

	}

	// logs end game data, increments resets, and saves results to database
	// only when "Play Again? Yes" button is clicked
	public void newGame() {
		plays++;
		reset();
		resultStr += "NEW_GAME,ice,";
	}

	// when the "Reset" button is clicked
	public void buttonReset() {
		resets++;
		plays++;
		logEndGameData();
		reset();
		resultStr += "OUTCOME,RESET,NEW_ATTEMPT,ice,";
	}

	// only when "I'm done playing" button is clicked
	// (end game data has not yet been logged)
	public void buttonQuit() {
		logEndGameData();
		resultStr += "OUTCOME,QUIT,END_SESSION,done,";
		SendSaveResult();
		SceneManager.LoadScene("postgame_survey");
	}

	// only when "Play Again? No" button is clicked
	// (end game data has already been logged)
	public void saveAndQuit() {
		resultStr += "END_SESSION,no,";
		SendSaveResult();
		SceneManager.LoadScene("postgame_survey");
	}

	private void SendSaveResult()
	{
		session_time = Time.time - sessionStart_time;
		resultStr += "ATTEMPTS," + plays + ",";
		resultStr += "RESETS," + resets + ",";
		resultStr += "VICTORIES," + victories + ",";
		resultStr += "SESSION_TIME," + session_time;
		GameObject.Find("DataCollector").GetComponent<dataCollector>().setPlayerData(resultStr);
		Debug.Log(resultStr);

	}

	public void reset() {
		if(victorious) {
			victorious = false;
		}
		transform.position = new Vector3(-4.5f,-6,0);
		square = new Vector2(6,2);
		predictedSquare = new Vector2(6,3);
		direction = Vector3.right;
		prevMoveDir = direction;
		transform.rotation = Quaternion.Euler(right);

		//Data collection variables
		actions = 0;
		moves = 0;
		turns = 0;
		pathTurns = 0;
		longest_straight_path = 0;
		pushes = 0;
		successfulPushes = 0;
		avg_time_per_move = -1f;
		startTime = Time.time;
		avg_turns_per_move = -1f;
		avg_turns_per_action = -1f;
		avg_turns_per_push = -1f;
		avg_path_turns_per_move = -1f;

		iceCantMove = 0; 	
		iceBlockedByIce = 0; 
		iceBlockedByOffscreen = 0; 
		iceStoppedByIce = 0; 
		iceStoppedByOffscreen = 0; 

		squares_explored_player_list = new List<string>();
		squares_explored_player_list.Add ("62");

		squares_explored_ice_list = new List<string>();
		squares_explored_ice_list.Add ("22");
		squares_explored_ice_list.Add ("52");
		squares_explored_ice_list.Add ("55");

		left_squares_player = 1;
		right_squares_player = 0;
		squares_explored_player = 1;
		num_repeated_squares_player = 0;
		num_traversed_squares_player = 1;

		left_squares_ice = 0; // because it will be calculated at end
		right_squares_ice = 0; // because it will be calculated at end
		top_squares_ice = 0; // because it will be calculated at end
		bottom_squares_ice = 0; // because it will be calculated at end
		squares_explored_ice = 0; // because it will be calculated at end
		num_repeated_squares_ice = 0; // because it will be calculated at end
		num_traversed_squares_ice = 0; // because it will be calculated at end

		left_right_symmetry_player = -1f;
		top_bottom_symmetry_player = -1f;
		left_right_symmetry_ice = -1f;
		top_bottom_symmetry_ice = -1f;

		pathTrace = coordinatesToSquare(square);

		foreach(iceBlock_4 i in ices) {
			i.reset(); 
		}

		unDisplayOptions();

	}

	private bool victory() {
		foreach(iceBlock_4 i in ices) {
			if(coordinatesToSquare(i.square).Equals ("34")) {
				foreach(iceBlock_4 j in ices) {
					if(coordinatesToSquare(j.square).Equals ("43")){
						victorious = true;
						return true;
					}
				}
			} 
		}
		return false;
	}

	// add square to count only once
	// create new array that is union of all 3 ice arrays
	private int iceNumSquaresExplored() {

		//create the union array to count each square only once
		int[,] ice1Squares = ices[0].getSquaresExplored();
		int[,] ice2Squares = ices[1].getSquaresExplored();
		int[,] union = new int[NUM_ROWS,NUM_COLS];
		for(int row = 0; row < NUM_ROWS; row++) {
			for(int col = 0; col < NUM_COLS; col++) {
				union[row,col] = ice1Squares[row,col] + ice2Squares[row,col];
				if(union[row,col] > 0) {
					union[row,col] = 1;
				}
			}
		}

		//actually count the number of unique squares explored
		int count = 0;
		for(int row = 0; row < NUM_ROWS; row++) {
			for(int col = 0; col < NUM_COLS; col++) {
				count += union[row,col];
			}
		}
		return count;
	}

	private void logActionData() {
		actions++;
		float currentTime = Time.time;
		float currentActionTime = currentTime - prevActionEndTime;
		avg_time_per_action += currentActionTime;
		prevActionEndTime = currentTime;
	}

	private void logMoveData() {
		moves++;
		float currentTime = Time.time;
		float currentMoveTime = currentTime - prevMoveEndTime;
		avg_time_per_move += currentMoveTime;
		prevMoveEndTime = currentTime;
	}

	private void logPushData() {
		pushes++;
		float currentTime = Time.time;
		float currentPushTime = currentTime - prevPushEndTime;
		avg_time_per_move += currentPushTime;
		prevPushEndTime = currentTime;
	}

	private void logEndGameData(){
		longest_straight_path = getLongestStraightPath(); //also calculates pathTurns value

		if(turns == 0) {
			avg_turns_per_action = -1f;
			avg_turns_per_move = -1f;
			avg_turns_per_push = -1f;
		} else {
			avg_turns_per_action = actions / (1.0f * turns);
			avg_turns_per_move = moves / (1.0f * turns);
			avg_turns_per_push = pushes / (1.0f * turns);
		}
	
		foreach(iceBlock_4 i in ices) {
			iceCantMove += i.getIceCantMove();
			iceBlockedByIce += i.getIceBlockedByIce();
			iceBlockedByOffscreen += i.getIceBlockedByOffscreen();

			iceStoppedByOffscreen += i.getStoppedByOffscreen();
			iceStoppedByIce += i.getStoppedByIce();
		}
		if(actions == 0) {
			avg_time_per_action = -1f;
		} else {
			avg_time_per_action = avg_time_per_action/actions;
		}
		if(moves == 0) {
			avg_time_per_move = -1f;
			avg_path_turns_per_move = -1f;
		} else {
			avg_time_per_move = avg_time_per_move/moves;
			avg_path_turns_per_move = pathTurns/(1.0f * moves);
		}
		float avg_push_success_rate;
		if(pushes == 0) {
			avg_time_per_push = -1f;
			avg_push_success_rate = -1f;
		} else {
			avg_time_per_push = avg_time_per_push/pushes;
			avg_push_success_rate = (1.0f * successfulPushes)/pushes;
		}
		game_time = (Time.time - startTime);

		squares_explored_player = getNumSquaresPlayerExplored(); // squares player has moved onto

		if(right_squares_player == 0) {
			left_right_symmetry_player = -1f;
		} else {
			left_right_symmetry_player = left_squares_player / (1.0f * right_squares_player);
		}

		if(bottom_squares_player == 0) {
			top_bottom_symmetry_player = -1f;
		} else {
			top_bottom_symmetry_player = top_squares_player / (1.0f * bottom_squares_player);
		}

		countIceSymmetry();
		squares_explored_ice += iceNumSquaresExplored();

		foreach(iceBlock_4 i in ices) {
			num_traversed_squares_ice += i.getTraversedSquares();
			num_repeated_squares_ice += i.getRepeatedSquares();
			successfulPushes += i.getSuccessfulPushes();
		}

		if(right_squares_ice == 0) {
			left_right_symmetry_ice = -1f;
		} else {
			left_right_symmetry_ice = left_squares_ice / (1.0f * right_squares_ice);
		}

		if(bottom_squares_ice == 0) {
			top_bottom_symmetry_ice = -1f;
		} else {
			top_bottom_symmetry_ice = top_squares_ice / (1.0f * bottom_squares_ice);
		}

		/* MOVEMENT DATA */
		resultStr += "ACTIONS," + actions +","; //should be equal to moves + pushes
		resultStr += "MOVES," + moves +",";
		resultStr += "TURNS," + turns +",";
		resultStr += "PATH_TURNS," + pathTurns + ",";
		resultStr +="LONGEST_STRAIGHT_PATH," + longest_straight_path + ",";
	
		resultStr += "PUSHES," + pushes +",";
		resultStr += "SUCCESSFUL_PUSHES," + successfulPushes +",";
		resultStr += "AVG_PUSH_SUCCESS_RATE," + avg_push_success_rate +",";
		resultStr += "AVG_TIME_PER_ACTION," + avg_time_per_action + ",";
		resultStr += "AVG_TIME_PER_MOVE," + avg_time_per_move + ",";
		resultStr += "AVG_TIME_PER_PUSH," + avg_time_per_push + ",";
		resultStr +="AVG_TURNS_PER_ACTION," + avg_turns_per_action+","; 
		resultStr +="AVG_TURNS_PER_MOVE," + avg_turns_per_move+",";
		resultStr +="AVG_TURNS_PER_PUSH," + avg_turns_per_push+",";
		resultStr +="AVG_PATH_TURNS_PER_MOVE," + avg_path_turns_per_move + ",";

		/* ICE BLOCK DATA */
		resultStr += "ICE_CANT_MOVE," + iceCantMove + ","; // total "errors"
		resultStr += "ICE_BLOCKED_BY_ICE," + iceBlockedByIce + ",";
		resultStr += "ICE_BLOCKED_BY_OFFSCREEN," + iceBlockedByOffscreen + ",";
		resultStr += "ICE_STOPPED_BY_ICE," + iceStoppedByIce + ",";
		resultStr += "ICE_STOPPED_BY_OFFSCREEN," + iceStoppedByOffscreen + ",";

		/* PLAYER LOCATION DATA */
		resultStr += "PLAYER_SQUARES_TRAVERSED," + num_traversed_squares_player + ",";
		resultStr += "PLAYER_SQUARES_EXPLORED," + squares_explored_player + ",";
		resultStr += "PLAYER_SQUARES_REPEATED," + num_repeated_squares_player + ",";

		resultStr += "PLAYER_LEFT_SQUARES," + left_squares_player + ",";
		resultStr += "PLAYER_RIGHT_SQUARES," + right_squares_player + ",";
		resultStr += "PLAYER_TOP_SQUARES," + top_squares_player + ",";
		resultStr += "PLAYER_BOTTOM_SQUARES," + bottom_squares_player + ",";
		resultStr += "PLAYER_LEFT_RIGHT_SYMMETRY," + left_right_symmetry_player + ",";
		resultStr += "PLAYER_TOP_BOTTOM_SYMMETRY," + top_bottom_symmetry_player + ",";

		/* ICE LOCATION DATA */
		resultStr += "ICE_SQUARES_TRAVERSED," + num_traversed_squares_ice + ",";
		resultStr += "ICE_SQUARES_EXPLORED," + squares_explored_ice + ",";
		resultStr += "ICE_SQUARES_REPEATED," + num_repeated_squares_ice + ",";

		resultStr += "ICE_LEFT_SQUARES," + left_squares_ice + ",";
		resultStr += "ICE_RIGHT_SQUARES," + right_squares_ice + ",";
		resultStr += "ICE_TOP_SQUARES," + top_squares_ice + ",";
		resultStr += "ICE_BOTTOM_SQUARES," + bottom_squares_ice + ",";
		resultStr += "ICE_LEFT_RIGHT_SYMMETRY," + left_right_symmetry_ice + ",";
		resultStr += "ICE_TOP_BOTTOM_SYMMETRY," + top_bottom_symmetry_ice + ",";	

		resultStr +="TOTAL_TIME," + game_time +",";
		resultStr +="PATH_TRACE," + pathTrace + ",";

	}

	private void countLeftRightSymmetry(string newLoc) {
		if(left_squares_list.Contains (newLoc)) {
			left_squares_player++;
		} else if (right_squares_list.Contains (newLoc)) {
			right_squares_player++;
		}
	}

	private void countTopBottomSymmetry(string newLoc) {
		if(bottom_squares_list.Contains (newLoc)) {
			bottom_squares_player++;
		} else if (top_squares_list.Contains (newLoc)) {
			top_squares_player++;
		}
	}

	private void countIceSymmetry() {
		foreach(iceBlock_4 i in ices) {
			int[,] iceSquaresExplored = i.getSquaresExplored();
			for(int row = 0; row < NUM_ROWS; row++) {
				for(int col = 0; col < NUM_COLS; col++) {
					string squareVisited = "" + (row + 1) + "" + (col + 1);
					if(left_squares_list.Contains(squareVisited)) {
						left_squares_ice += iceSquaresExplored[row,col];
					} else if(right_squares_list.Contains(squareVisited)) {
						right_squares_ice += iceSquaresExplored[row,col];
					}

					if(top_squares_list.Contains(squareVisited)) {
						top_squares_ice += iceSquaresExplored[row,col];
					} else if(bottom_squares_list.Contains(squareVisited)) {
						bottom_squares_ice += iceSquaresExplored[row,col];
					}

				}
			}
		}

	}


		
	// Update is called once per frame
	void Update () {
		if(!victorious) {
			if(victory()) {
				logEndGameData ();
				resultStr +="OUTCOME,VICTORY,";
				victories++;
				displayOptions();
			}else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				if(approximately(direction, Vector3.down)) {
					// move down
					tryMove();
				} else {
					// turn down
					turns++;
					turnDown ();
				}
			} else if (Input.GetKeyDown (KeyCode.UpArrow)) {
				if(approximately(direction, Vector3.up)) {
					// move up
					tryMove();
				} else {
					// turn up
					turns++;
					turnUp ();
				}
			} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
				if(approximately(direction, Vector3.right)) {
					// move right
					tryMove();
				} else {
					// turn right
					turns++;				
					turnRight ();
				}
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				if(approximately(direction, Vector3.left)) {
					// move left
					tryMove();
				} else {
					// turn left
					turns++;
					turnLeft ();
				}
			} 
		}

	}

	private void push(iceBlock_4 ice) {
		string newLoc = ice.move();
	}

	private bool blockedByIce() {
		foreach(iceBlock_4 i in ices) {
			if(predictedSquare == i.square) {
				return true;
			}
		}
		return false;
	}

	private void tryMove() {
		logActionData ();
		bool[] errorsPlayer = getErrorType ();
		if(!errorsPlayer[0] && !errorsPlayer[1]) {
			// player moves physically in the direction they are turned
			logMoveData();
			if(!approximately(prevMoveDir, direction)) {
				pathTurns++;
				Debug.Log("path turned! pathTurns = " + pathTurns);
				prevMoveDir = direction;
			}
			string newLoc = move();
			countLeftRightSymmetry(newLoc); // includes repetitions
			countTopBottomSymmetry(newLoc); //includes repetitions

		} else if(errorsPlayer[1]) {
			//Player blocked by ice, move ice if possible
			logPushData();
			foreach(iceBlock_4 i in ices) {
				if(predictedSquare == i.square) {
					push(i);
				}

			}

		} 
	}
}
