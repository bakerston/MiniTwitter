# COP5615. Project 4, Part I

Chi Zhang, 9183-2967

Jianan He, 6853-0029

---

## Description:

In this project, we built a tiny social network service (SNS), similar in concept with posting and receiving status updates on Twitter and implemented a routing server and client servers to maintain availability. 

- Implemented Twitter-like engine with the following functionality: 
  - Register account. 
  - Send tweets with **arbitrary** number of hashtags and mentions.
  - Subscribe to user's tweets.
  - Re-tweets.
  - Query by subscribed to, hashtags or mentions.
- Simulate Zipf's Law  on number of subscribers. 
- Measure various aspects of the simulator as required.



## Environment and Machine:

- F#
- Akka
  - Akka 1.4.25
  - Akka.FSharp 1.4.25
  - Akka.Remote 1.4.25

- ASP.Net core 5.0.10

- Visual Studio 2019
- MacBook Air Apple M1 Chip with 8-Core CPU
  - OS Name:     Mac OS X
- Thinkpad T480s 1.9 GHz 4 Cores Intel i7
  - OS Name: Windows 10 Home

## Introduction:

- Run `pro4/Program.fs`, the remote server first. 

  Rune `client/Program.fs`, the remote client.

  Input the number of users.

  ![Input](https://github.com/bakerston/MiniTwitter/tree/main/src/Input.jpg)

**The current code prints all response from server and client for all required operation. However, we commented some part of the code for testing for the sake of brevity. In order to test the performance as we did in the report, please comment out the related test code.**

- #### The message format:

  The message of operation consists of 7 parts separated by comma, the format is showed below:

  ```[op_tp],[un_1],[t_cont],[t_id],[un_2],[tag],[men] ```

  `op_tp` stands for **operation type**, includes all 7 operations.

  ​				[0] Registration.

  ​				[1] Subscribe

  ​				[2] Post tweet

  ​				[3] Re-tweet

  ​				[4] Query by user

  ​				[5] Query by hashtag

  ​				[6] Query by mentions

  `[un_1]` stands for the username_1, the current username.

  `[t_cont]` stands for the tweet content.

  `[t_id]` stands for the tweet's id.

  `[un_2]` stands for the username_2, the target username.

  `[tag]` stands for the hashtags, a string of one/multiple hashtags separated by **#**.

  `[men]` stands for the mentions, a string of one/multiple username being mentioned in the tweet, separated by **@**.

  

  Each block is filled according to the message type, and joined by comma to form the final message, the block without information is left as blank. 

   

  [0] Registration: [un_1]

​				[1] Subscribe:  [un_1] [un_2]

​				[2] Post tweet: [un_1] [t_cont] [tag] [men]

​				[3] Re-tweet: [un_1] [t_id]

​				[4] Query by user: [un_1]

​				[5] Query by hashtag: [tag]

​				[6] Query by mentions: [men]

- #### System Architecture.

  ![DOSP_pro_3-dosp_project4_part1.drawio](https://github.com/bakerston/MiniTwitter/tree/main/src/DOSP_pro_3-dosp_project4_part1.drawio.png)

  As showed above, both the remote client and remote server have multiple actors. Here, 

  1: Message for registering users delivered to Handler on Server side. 

  2: Message requiring the other 6 operations delivered to Handler on Server side. 

  3 & 4: **Retweet** operation, the generator will firstly acquire a random tweet id from Remote Server, then builds message with that tweet id, and finally deliver the operation message to Remote Server as described in 2.

  5: Message from Handler split by operation type is delivered to accordingly Operation Actors.

  6: Response from Operation Actor on Server side is delivered to the Msg Generator Actor on Client side.

  The general path is that:

  -  On Client side, a message following the previous rules is generated and is delivered to Handler in Server.
  -  The Handler (orange) will split and analyze message and retransmit it to target actors (colored in red) based on the operated type needed.
  - Once the functionality actor finish the operation, the response message is delivered back to Msg Generation Actor on Client side.

- #### Examples.

  - Registration

    Input: `[reg],[username],,,,,`

    Response:

    ![Reg](https://github.com/bakerston/MiniTwitter/tree/main/src/Reg.png)

  - Subscription

    Input: `[sub],[username1],,,[username2],,`

    Response:

    ![Sub](https://github.com/bakerston/MiniTwitter/tree/main/src/Sub.png)

  - Tweet

    Input: `[send],[username],[tweet_content],,,[tags],[mentions]`

  - Query by User

    Input: `[query],[username],,,,,`

    Response:

    ![QueryUser](https://github.com/bakerston/MiniTwitter/tree/main/src/QueryUser.png)

  - Query by Mentions

    Input: `[querm],,,,,,[mention]`

    Response:

    ![QueryMention](https://github.com/bakerston/MiniTwitter/tree/main/src/QueryMention.png)

  - Query by Tags

    Input: `[querm],,,,,[tag],`

    Response:

    ![QueryTag](https://github.com/bakerston/MiniTwitter/tree/main/src/QueryTag.jpg)

  - Retweet

    Input: `[retw],,,[user_id],,,`

    Response:

    ![Retweet](https://github.com/bakerston/MiniTwitter/tree/main/src/Retweet.jpg)

## Result.

- Zipf's Law on number of subscribers. Here we simulate the distribution among **100** users.

  ![exe_1](https://github.com/bakerston/MiniTwitter/tree/main/src/exe_1.png)

- The maximum number of users we tested is 1000. 

- The overall time (ms) of each operation according to the number of total users.

| num of users | reg    | sub    | tweet   | retweet  | Q_user | Q_tag | q_men |
| ------------ | ------ | ------ | ------- | -------- | ------ | ----- | ----- |
| 5            | 663.9  | 31.7   | 605.8   | 3145.6   | 1014   | 69.9  | 52.9  |
| 10           | 682.3  | 66     | 1097.8  | 8159.7   | 1015.1 | 58.3  | 63.2  |
| 50           | 706.4  | 108.6  | 5056.1  | 44199.3  | 1006.5 | 52.7  | 62.9  |
| 100          | 999.2  | 214.5  | 9962.3  | 89257.5  | 1012.1 | 63.2  | 60.9  |
| 500          | 4922.6 | 1086.5 | 49412.6 | 449651.1 | 1004.3 | 54.7  | 64.1  |
| 1000         | 9841.6 | 2153   | 98664.9 | 866574   | 990.9  | 54.9  | 41.8  |

![performance](https://github.com/bakerston/MiniTwitter/tree/main/src/performance.png)



- Performance of randomly selected operations.

| Number of users | random |
| --------------- | ------ |
| 5               | 304.6  |
| 10              | 310.8  |
| 50              | 405    |
| 100             | 400.5  |
| 500             | 294.9  |
| 1000            | 291.6  |

![random](https://github.com/bakerston/MiniTwitter/tree/main/src/random.png)

We can tell that given the number of operations (M), the overall time doesn't depend heavily on the total number of users (N), Since each single operation contains constant number of message deliveries and calculations which are not in proportional to the number of users, thus the operation time is stable regardless of the community size.
