---
title: Dependency Injection
---

<section>
    <p>
        Луд умора няма. 
        Вчера от нямане какво да правя реших – абе дай да си направя един простичък Dependency Injector. 
        И преди да опиша какво е това, мисля, че е по-добре да разясня какво е “обръщане на контрола” (Inversion of Control – IoC) 
        и “инжектиране на зависимости” (dependency injection).
    </p>
</section>

<section>
    <h2>Inversion of Control</h2>
    <p>
        Когато пишем проектите си, използвайки някой обектно-ориентиран език 
        много често обичаме да пишем “конкретно”, а не “абстрактно”. 
        Тоест обичаме да зависим на конкретни имплементации, а не на абстракции. 
        Пример за това е, ако да речем имаме един клас <strong>Lamp</strong> и един бутон за нейното включване – клас <strong>Button</strong>. 
    </p>
    <p>
        По-неопитните с ООП биха предложили да направим директна връзка между между лампата и бутона, 
        като по този веднага бихме създали една зависимост. 
        За да оперира коректно нашата лампа тя зависи от един бутон, чиято единствена цел би била да я включи. 
        И това определено не е добра идея. 
    </p>
    <p>
        Вместо можем да си изнесем един интерфейс (да го кръстим <strong>ILightSwitcher</strong>, например), 
        чиято единствена цел би била да се разпорежда с включването на лампата. 
        По този начин разкачваме двата класа и премахваме зависимостта между двата класа – и двата ще зависят от абстракция, 
        а не от конкретни имплементация. 
        Това е което наричаме обръщане на контрола – когато нашите модули от високо ниво (каквато е лампата ни), 
        не зависят от модули от по-ниско ниво (какъвто е нашият бутон). И двете зависят от абстракции.
    </p>
</section>

<section>
    <h2>Dependency Injection</h2>
    <p>
        Какво представлява “инжектирането на зависимости” – това представлява шаблон за дизайн, 
        който ни позволява да остраним здравите връзки между класовете и да ги подменяме по време на компилация или по дори 
        и по време на изпълнението на нашата програма. При инжектирането на зависимости се открояват три елемента:
    </p>
    <ul>
        <li>Зависим елемент</li>
        <li>Декларация на зависимости – различни типове договори (интерфейси, абстрактни класове)</li>
        <li>Контейнер(container/resolver) на зависимостите</li>
    </ul>
    <p>
        Какво ще рече това? 
        Главната мотивация за създаването на този шаблон е да могат да се променят конкретните имплементации, 
        които се използват по време на изпълнение на програмата, а не при компилиране. 
        Това позволява кодът, който пишем да е много по модуларен и разкачен, което го прави в същото време и много по-тестваем.
    </p>
</section>

<section>
    <h2>Примерна имплементация на Dependency Injector</h2>
    <p>
        Има много различни IoC контейнери (например Ninject, Autofac за .NET или Pico, Guice за Java). 
        И всъщност основната им имплементация не е много сложна – имаме един речник, 
        в който ключа е типът на зависимостта, а стойността е конкретният тип на разрешението й. 
        Нека започнем така:
    </p>

    {% highlight c# %} 
    public class Container
    {
        private IDictionary<Type, Type> dependencies;
        private ContainerOptions options;

        private const ContainerOptions defaultOptions = ContainerOptions.None;

        public Container() : this(defaultOptions)
        {
        }

        public Container(ContainerOptions options)
        {
            this.options = options;
            this.dependencies = new Dictionary<Type, Type>();
        }
    }
    {% endhighlight %}

    <p>
        <strong>ContainerOptions</strong> е просто енумерация, която, въпреки че не използва много в текущия проект, 
        бихме могли да я използваме за да конфигурираме допълнитено начина, по който разрешаваме зависимостите.
    </p>

    <p>
        След като сме дефинирали основните ни променливи и конструктори, които ще използваме, 
        ни трябва начин да регистрираме зависимости и как да те да бъдат разрешавани. 
        Тоест, двойка типове, които ни показват кой клас да се инстанцира, когато ни е необходим конкретна имплементация на даден интерфейс.
    </p>

    {% highlight c# %} 
    public void RegisterType<TDependencyType, TResolveType>()
        where TDependencyType : class
        where TResolveType : class
    {
        this.dependencies.Add(typeof(TDependencyType),typeof(TResolveType));
    }
    {% endhighlight %}

    <p>
        Единственото, което ни остава е все пак да разрешим зависимостта. 
        Ще го направим рекурсивно, използвайки алгоритъма за обхождане в дълбочина. 
        По този начин ще си гарантираме, че ако някое от нашите разрешения има свои зависимости, бихме могли да ги разрешим и тях.
    </p>

    {% highlight c# %} 
    public T Resolve<T>() where T : class
    {
        var constructors = typeof(T)
            .GetConstructors()
            .OrderByDescending(x => x.GetParameters().Count());

        if (!constructors.Any())
        {
            throw new ArgumentException("The class to be resolved does not have any public constructors!");
        }

        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                return Activator.CreateInstance<T>();
            }
            else
            {
                List<object> parameterObjects = new List<object>();

                foreach (var parameter in parameters)
                {
                    var parameterType = parameter.ParameterType;

                    if (this.options.HasFlag(ContainerOptions.UseDefaultValue))
                    {
                        if (parameter.HasDefaultValue)
                        {
                            var res = Convert.ChangeType(parameter.DefaultValue, parameterType);

                            parameterObjects.Add(res);

                            continue;
                        }
                    }

                    if (parameterType.IsAbstract || parameterType.IsInterface)
                    {
                        var concreteObjectType = this.dependencies[parameterType];
                        var method = typeof(Container).GetMethod("Resolve").MakeGenericMethod(concreteObjectType);
                        var obj = method.Invoke(this, null);

                        parameterObjects.Add(obj);
                    }
                    else if (parameterType.IsPrimitive || parameterType.GetConstructors().Any(x => !x.GetParameters().Any()))
                    {
                        var obj = Activator.CreateInstance(parameterType);

                        parameterObjects.Add(obj);
                    }
                }

                if (parameterObjects.Count != constructor.GetParameters().Length)
                {
                    continue;
                }

                var createdObject = (T)Activator.CreateInstance(typeof(T), parameterObjects.ToArray());

                return createdObject;
            }
        }

        throw new Exception("Could not resolve the dependency");
    }
    {% endhighlight %}

    <p>
        Трябва, обаче да се има предвид, че това е непълна имплементация, 
        която може да се разглежда като интересно упражнение и да изясни някои неясноти относно IoC контейнерите, 
        по какъв начин действат и какво представлява dependency inversion. 
        Пълният код може да бъде намерен <a href="https://github.com/csyntax/IoC-Container">тук</a>.  
    </p>
</section>