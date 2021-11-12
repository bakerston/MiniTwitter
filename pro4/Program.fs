open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration

////////////////////////////////////////////////////////
// 1. CLIENT -----message[,,,,,]-----> SERVER.HANDLER //
////////////////////////////////////////////////////////
// 2. SERVER.HANDLER SPLITS AND ANAYLSES MESSAGE ///////
////////////////////////////////////////////////////////
// 3. SERVER.HANDLER -----functMsg------> functActor  //
////////////////////////////////////////////////////////
// 4. functActor PROCESSES functMsg  ///////////////////
////////////////////////////////////////////////////////

// regMsg: registration; subMsg: subscribe; sendMsg: post tweet;
// retwMsg: retweet;       queryMsg: query by user;
// quertMsg: query by tag; quermMsg: query by mention.

/////////////////////////////////////////////////////////
// Format of CLIENT MESSAGE:  ///////////////////////////
// 0. operation [reg, sub, send, ...]        ////////////
// 1. usrename        2. tweet_content       //////////// 
// 3. tweet_id        4. user to subscribe   ////////////
// 5. string of tags  6. string of mentions  ////////////
/////////////////////////////////////////////////////////

type regMsg = {
    username: String    // username to register    
}
type subMsg = {
    username: String    // username of subscriber
    target: String      // whom to subscribe
}
type sendMsg = {
    username: String   // username of the poster 
    tweet_cont: String // tweet_content
    tag_string: String // string containing all tags, split by "#"
    men_string: String // string containing all mentions, split by "@
}
type retwMsg = {
    username: String    // username of the poster
    tweet_id: String    // id of the tweet TO BE RETWEETED.
}
type queryMsg = {
    user: String        // user to query
}
type quertMsg = {
    tag: String         // tag to query
}
type quermMsg = {
    mention: String     // mention to query
}
type Tweet(id: string, content: string) =
    let mutable tag_list = List<String>.Empty     // List of tags in this tweet
    let mutable mention_list = List<String>.Empty // List of mentions in this tweet
    member this.id = id
    member this.content = content
    member this.set_tag taglist = 
        tag_list <- taglist
    member this.set_men menlist = 
        mention_list <- menlist
    member this.get_tag =
        tag_list
    member this.get_men = 
        mention_list

type User(username: string) =
    let mutable subscribe_list = List<String>.Empty // List of subscribed names of this user
    let mutable tweet_list = List<String>.Empty     // List of tweet ids of this user
    member this.username = username
    member this.subscribe(target_username: String) =
        if target_username = username then
            printfn "You Cannot Subscribe Yourself!"
        else
            subscribe_list <- [target_username] |> List.append(subscribe_list)
    member this.getSubscriberList =
        subscribe_list
    member this.addTweet(tweetid: String) =
        tweet_list <- [tweetid] |> List.append(tweet_list)
    member this.getTweetList =
        tweet_list


let mutable user_total = new Map<String, User>([])           // <user_name,       user_obj>
let mutable tweet_total = new Map<String, Tweet>([])         // <tweet_id,        tweet_obj>
let mutable tag_total = new Map<String, String list>([])     // <tag_content,     list of tweet_id>
let mutable mention_total = new Map<String, String list>([]) // <mention_content, list of tweet_id>

//let verify username password = 
//    let mutable valid = false
//    if users.ContainsKey(username) then
//        let user = users.[username]
//        if password = user.password then
//            valid <- true
//    valid

let register username = 
    let mutable resp = ""
    if user_total.ContainsKey(username) then
        resp <- "Username Already Taken!"
    else
        let user = new User(username)
        user_total <- user_total.Add(user.username, user)
        user.subscribe username
        resp <- "Registration of :" + username + "Success!"
    resp // done

let splitTag = (fun (line : string) -> Seq.toList (line.Split "#"))
let splitMen = (fun (line : string) -> Seq.toList (line.Split "@"))

// tag_string Format:     TagA#TagB#TagC...#TagX
// mention_string Format: MenA#MenB#MenC...#MenX
let send username tweet_cont tag_string men_string =
    let mutable resp = ""
    if not (user_total.ContainsKey(username)) then
        resp <- "User Not Found!"
    else
        let tweetid = (System.DateTime.Now.ToFileTimeUtc()|> string) + username
        let tweet = new Tweet(tweetid, tweet_cont)
        let user = user_total.[username]
        user.addTweet tweetid
        tweet_total <- tweet_total.Add(tweetid, tweet) 
        let mutable prevlist = List<String>.Empty 
        let mutable tmplist = List<String>.Empty 
        let mutable idx = 0
        if not (String.Empty = tag_string) then
            let tags = splitTag tag_string
            tweet.set_tag tags
            let tlen = List.length tags
            idx <- 0
            let mutable curtag = ""           
            while idx < tlen do
                curtag <- tags.[idx]
                if not (tag_total.ContainsKey(curtag)) then
                    tmplist <- List<String>.Empty 
                    tag_total <- tag_total.Add(curtag, tmplist)
                prevlist <- tag_total.[curtag]
                prevlist <- [tweetid] |> List.append prevlist
                tag_total <- tag_total.Add(curtag, prevlist)
                idx <- idx + 1
        if not (String.Empty = men_string) then
            let mens = splitTag men_string
            tweet.set_men mens
            let mlen = List.length mens
            idx <- 0
            let mutable curmen = ""
            while idx < mlen do
                curmen <- mens.[idx]
                if not (mention_total.ContainsKey(curmen)) then
                    tmplist <- List<String>.Empty 
                    mention_total <- mention_total.Add(curmen, tmplist)
                prevlist <- mention_total.[curmen]
                prevlist <- [tweetid] |> List.append prevlist
                tag_total <- tag_total.Add(curmen, prevlist)
                idx <- idx + 1 // done      

let subscribe user1 user2 = // string, string
    let mutable resp = ""
    if not (user_total.ContainsKey(user1) && user_total.ContainsKey(user2)) then
        resp <- "User1 or User2 Not Found!" 
    else
        let user = user_total.[user1]
        user.subscribe user2
        resp <- user1 + " Subscribed " + user2 + "Successfully!"
    resp // done

let retweet user tweet_id = // string, string
    let mutable resp = ""
    if not (user_total.ContainsKey(user)) then
        resp <- "User Not Found!"
    else if not (tweet_total.ContainsKey(tweet_id)) then
        resp <- "Tweet Not Found!"
    else 
        let old_tweet = tweet_total.[tweet_id]
        let old_content = old_tweet.content
        let new_id = user + tweet_id
        let user = user_total.[user]
        let new_tweet = new Tweet(new_id, old_content)
        user.addTweet new_id

        let old_tag_list = old_tweet.get_tag
        let old_men_list = old_tweet.get_men       
        new_tweet.set_tag old_tag_list
        new_tweet.set_men old_men_list

        tweet_total <- tweet_total.Add(new_id, old_tweet)

        let mutable idx = 0
        let mutable prevlist = List<String>.Empty 
        if not (List.Empty = old_tag_list) then
            let tlen = List.length old_tag_list           
            let mutable curtag = ""  
            idx <- 0
            while idx < tlen do
                curtag <- old_tag_list.[idx]
                prevlist <- tag_total.[curtag]
                prevlist <- [new_id] |> List.append prevlist
                tag_total <- tag_total.Add(curtag, prevlist)
                idx <- idx + 1
        if not (List.Empty = old_men_list) then
            let mlen = List.length old_men_list           
            let mutable curmen = "" 
            idx <- 0
            while idx < mlen do
                curmen <- old_men_list.[idx]
                prevlist <- mention_total.[curmen]
                prevlist <- [new_id] |> List.append prevlist
                mention_total <- mention_total.Add(curmen, prevlist)
                idx <- idx + 1 // done 
                
let query username = 
    let mutable resp = ""
    if not (user_total.ContainsKey(username)) then
        resp <- "User Not Found!"
    else
        let user = user_total.[username]    
        let res1 = user.getSubscriberList |> List.map(fun x -> user_total.[x]) |> List.map(fun x -> x.getTweetList) |> List.concat |> List.map(fun x->tweet_total.[x])|> List.map(fun x -> x.content) |> String.concat "\n"
        resp <- "Tweets Subscribed :" + res1
    resp // done
        //let sub_username_list = user.getSubscriberList
        //let sub_user_list = List.map(fun x -> user_total.[x]) sub_username_list
        //let sub_tweet_lists = List.map(fun x -> x.getTweetList) sub_user_list
        //let 
        // let sub_tweet_list = List.concat sub_tweet_lists
        //let output_string = sub_tweet_list |> String.concat "//"
        //resp <- "Tweets Subscribed :" + output_string

let quert tag = 
    let mutable resp = ""
    if not (tag_total.ContainsKey(tag)) then
        resp <- "Tag Not Found!"
    else
        let res1 = tag_total.[tag] |> List.map(fun x -> tweet_total.[x]) |> List.map(fun x -> x.content) |> String.concat "\n"
        resp <- "Tweet containing Tag :" + res1 // done

let querm men = 
    let mutable resp = ""
    if not (mention_total.ContainsKey(men)) then
        resp <- "Mention Not Found!"
    else
        let res1 = mention_total.[men] |> List.map(fun x -> tweet_total.[x]) |> List.map(fun x -> x.content) |> String.concat "\n"
        resp <- "Tweet containing Mention :" + res1 // done

let system = System.create "Project4" (Configuration.load())

let createRegActor () =
    spawn system ("Actor" + "retw")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? regMsg as msg ->
                    let username = msg.username
                    let res = register username

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )           // done

let createSubActor () =           
    spawn system ("Actor" + "retw")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? subMsg as msg ->
                    let user = msg.username
                    let target_user = msg.target
                    let res = subscribe user target_user

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )// done

let createSendActor () = 
    spawn system ("Actor" + "retw")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? sendMsg as msg ->
                    let user = msg.username
                    let tweet_cont = msg.tweet_cont
                    let tag_string = msg.tag_string
                    let men_string = msg.men_string
                    let res = send user tweet_cont tag_string men_string

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )         // done

let createRetwActor () =
    spawn system ("Actor" + "retw")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? retwMsg as msg ->
                    let user = msg.username
                    let tweet_id = msg.tweet_id
                    let res = retweet user tweet_id

                    sender <? res |> ignore
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )          // done

let createQueryActor () =
    spawn system ("Actor" + "query")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? queryMsg as msg ->
                    let user = msg.user
                    let res = query user

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )           // done

let createTagActor () =
    spawn system ("Actor" + "tag")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? quertMsg as msg ->
                    let tag = msg.tag
                    let res = quert tag

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )

let createMentionActor () =
    spawn system ("Actor" + "mention")
        (fun mailbox ->          
            let buildTime = Diagnostics.Stopwatch()
            buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->
                    printfn "%s" msg
                | :? quermMsg as msg ->
                    let men = msg.mention
                    let res = querm men

                    sender <? res |> ignore
                    Threading.Thread.Sleep(10)
                | _ -> () 
                return! loop()
            }
            loop()
        )

let Handlers (num: int) =
    spawn system ("Actor" + num.ToString())
        (fun mailbox ->          
            //let buildTime = Diagnostics.Stopwatch()
            //buildTime.Start()
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()             
                match box message with
                | :? int as msg ->
                    printfn "%i" msg
                | :? string as msg ->                   
                    let result = msg.Split ','
                    let mutable operation = result.[0]
                    let mutable username = result.[1]
                    let mutable tweet_content = result.[2]
                    let mutable tweet_id = result.[3]
                    let mutable dest_user = result.[4]
                    let mutable tag = result.[5]
                    let mutable mention = result.[6]
                    match operation with 
                    | "reg"  ->      
                        let newMessage = {username = username}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "reg")
                        destActor <! newMessage                      
                    | "send" ->
                        let newMessage = {username = username; tweet_cont = tweet_content; tag_string = tag; men_string = mention}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "send")
                        destActor <! newMessage
                    | "sub"  ->
                        let newMessage = {username = username; target = dest_user}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "sub")
                        destActor <! newMessage
                    | "retw" ->
                        let newMessage = {username = username; tweet_id = tweet_id}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "retw")
                        destActor <! newMessage
                    | "query"  ->
                        let newMessage = {user = username}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "query")
                        destActor <! newMessage
                    | "quert" ->
                        let newMessage = {tag = tag}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "quert")
                        destActor <! newMessage
                    | "querm"  ->
                        let newMessage = {mention = mention}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "querm")
                        destActor <! newMessage
                    | _ -> () 
                | _ -> () 
                return! loop()
            }
            loop()
        )


[<EntryPoint>] 
let main argv =
    printfn "%A" argv
    // once we received a set of string, dispatch to different functional actor
    // dispatch was based on the opt.

    printfn "------------------------------------------------- \n " 
    printfn "-------------------------------------------------   " 
    printfn "Twitter Server is running...   " 
    printfn "-------------------------------------------------   "
    
    // For function reg
    Console.ReadLine() |> ignore
   
    printfn "-----------------------------------------------------------\n" 
    0