open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration

// User, Tweet Type --He
// Handler --Chi

type regMsg = {
    username: String
    password: String
}

type subMsg = {
    username: String
    password: String
    target: String
}

type sendMsg = {
    username: String
    password: String
    tweet_cont: String // tweet_content
}

type retwMsg = {
    username: String
    password: String
    tweet_id: String // tweet_id
}

type queryMsg = {
    username: String
    password: String
}
type quertMsg = {
    tag: String
}
type quermMsg = {
    mention: String
}

type Tweet(id: string, content: string) =
    member this.id = id
    member this.content = content
    override this.ToString() =
        let mutable res = ""
        if isRetweet then
            res <- "[retweet]" + "id = " + this.id + " content = " + this.content
        else
            res <- "id = " + this.id + " content = " + this.content
        res
type User(username: string, password: string) =
    let mutable subscribeList = List<User>.Empty
    let mutable tweetList = List<String>.Empty

    member this.username = username
    member this.password = password

    member this.addSubscriber(user: User) =
        if user.username = username && user.password = password then
            printfn "cannot subscribe self!"
        else
            subscribeList <- [user] |> List.append(subscribeList)
    member this.getSubscriberList =
        subscribeList
    member this.addTweet(tweetid: String) =
        tweetList <- [tweetid] |> List.append(tweetList)
    member this.getTweetList =
        tweetList
    override this.ToString() =
        let res = "username = " + this.username
        res


let mutable users = new Map<String, User>([])          // <user_name, user_obj>
let mutable tweets = new Map<String, Tweet>([])        // <tweet_id, tweet_obj>
let mutable tags = new Map<String, String list>([])     // <tag_content, list of tweet_id>
let mutable mentions = new Map<String, String list>([]) // <mention_content, list of tweet_id>

let verify username password = 
    let mutable valid = false
    if users.ContainsKey(username) then
        let user = users.[username]
        if password = user.password then
            valid <- true
    valid

let register username password = 
    let mutable resp = ""
    if users.ContainsKey(username) then
        resp <- "Username Already Taken!"
    else
        let user = new User(username, password)
        users <- users.Add(user.username, user)
        user.addSubscriber user
        resp <- "Registration of :" + username + "Success!"
    resp

let send username password tweet_cont =
    let mutable resp = ""
    if not (users.ContainsKey(username)) then
        resp <- "No User Found!"
    else
        if not (verify username password) then
            resp <- "Password Not Correct!"
        else
            let tweetid = (System.DateTime.Now.ToFileTimeUtc()|> string) + username
            let tweet = new Tweet(tweetid, tweet_cont)
            let user = users.[username]
            user.addTweet tweetid
            tweets <- tweets.Add(tweetid, tweet)


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
                    let password = msg.password
                    let res = register username password
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )  // done
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
                    
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )
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
                    let res = send username password tweet_cont
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )

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
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )

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
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )

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
                    Threading.Thread.Sleep(1000)
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
                    Threading.Thread.Sleep(1000)
                | _ -> () 
                return! loop()
            }
            loop()
        )


// Actors & operation [reg,send,sub--He | retw,query,quert,querm--Chi]
// command: operation + username + password + tweet + she/he + #tags + @mention


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
                    let mutable password = result.[2]
                    let mutable tweet_content = result.[3]
                    let mutable dest_user = result.[4]
                    let mutable tag = result.[5]
                    let mutable mention = result.[6]
                    match operation with 
                    //| "reg"  ->                 
                    //| "send" ->
                    | "sub"  ->
                        let newMessage = {user = username; password = password; tweet = tweet_content}
                    | "retw" ->
                        let newMessage = {user = username; password = password; tweet = tweet_content}
                        let destActor = system.ActorSelection("akka://Project4/user/Actor" + "retw")
                        destActor <! newMessage
                    | "query"  ->
                        let newMessage = {user = username; password = password}
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