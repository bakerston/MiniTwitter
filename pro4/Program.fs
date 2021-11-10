open System
open Akka.FSharp
open System.IO
open System.Security.Cryptography
open Akka.Configuration

// User, Tweet Type --He
// Handler --Chi
type retwMsg = {
    user: String
    password: String
    tweet: String
}
type queryMsg = {
    user: String
    password: String
}
type quertMsg = {
    tag: String
}
type quermMsg = {
    mention: String
}

type Tweet(tweet_id:string, text:string, is_re_tweet:bool) =
    member this.tweet_id = tweet_id
    member this.text = text
    member this.is_re_tweet = is_re_tweet
//    member this.time = System.DateTime.Now.ToFileTimeUtc() |> string
    override this.ToString() =
      let mutable res = ""
      if is_re_tweet then
        res <- sprintf "[retweet][%s]%s" this.tweet_id this.text
      else
        res <- sprintf "[%s]%s" this.tweet_id this.text
//        res <- sprintf "%s" this.text
      res
type User(user_name:string, password:string) =
    let mutable subscribes = List.empty: User list
    let mutable tweets = List.empty: Tweet list
    member this.user_name = user_name
    member this.password = password
    member this.addSubscribe x =
        subscribes <- List.append subscribes [x]
    member this.getSubscribes() =
        subscribes
    member this.addTweet x =
        tweets <- List.append tweets [x]
    member this.getTweets() =
        tweets
    override this.ToString() = 
       this.user_name

let mutable users = new Map<String, User>([])          // <user_name, user_obj>
let mutable tweets = new Map<String, Tweet>([])        // <tweet_id, tweet_obj>
let mutable tags = new Map<String, Tweet list>([])     // <tag_content, list of tweet_obj>
let mutable mentions = new Map<String, Tweet list>([]) // <mention_content, list of tweet_obj>

let system = System.create "Project4" (Configuration.load())

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
                    //| "sub"  ->
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