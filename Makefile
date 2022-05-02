VERSION=$(shell date +"%Y%m%d_%H%M")

aid.tar.gz: AidUkraine
	dotnet.exe clean -c Release $<
	dotnet.exe publish -c Release -r linux-x64 --no-self-contained $<
	tar cfz $@ -C $< Dockerfile.aid -C bin/Release/net6.0/linux-x64/publish .

publish: aid.tar.gz
	scp $^ $(TARGET):~/aid.$(VERSION).tar.gz
	$(eval REGISTRY := r.$(lastword $(subst @, ,$(TARGET))))
	ssh $(TARGET) "tar xfz aid.$(VERSION).tar.gz --one-top-level && docker build aid.$(VERSION) -f aid.$(VERSION)/Dockerfile.aid -t $(REGISTRY)/aid_matcher:$(VERSION) && docker push $(REGISTRY)/aid_matcher:$(VERSION)"

deploy:
	envsubst < AidUkraine/k8s.yaml | kubectl apply -f -
