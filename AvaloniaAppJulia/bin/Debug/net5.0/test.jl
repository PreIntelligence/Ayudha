module test
using ForwardDiff
#println("Loading the Module")



export TestFunc, Func1, Func3, Func2
export @ab, @u

export foo!, mySquare
foo!(x) = (x*=2) # multiply entries of x by 2 inplace

function TestFunc()
    println("In the function")
end

function Func1(x)
     println("This அம்ம function returns 4")
     return 4
end


function Func2(x)


    f(x::Vector) =  prod(log, x) * sum(sqrt, x);
    x = rand(1,10)
    g = x -> ForwardDiff.gradient(f, x); # g = ∇f


    x=100 .*rand(100)
    #return string(sin.(x))*"#"*string(g(x)) #"அம்மா"
    x = collect(10:.01:30)
    y = log.(x .+ sin.(x))
    z = (x .+ cos.(x))

    return string(y)*"#"*string(z) #"அம்மா"
end

function Func3(x)
     return (rand(1,10))
end

function mySquare(a)
       return sum(a)
end




macro ab(n)
    quote
    println($(esc(n)))
    "அம்மா"
    end
end

macro u(n, body)
  quote
    for i = 1:$(esc(n))
      println($(esc(body)))
    end
      "அம்மா"
  end
end







end
